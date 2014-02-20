using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Diagnostics;
using Neuroflow.Core;

namespace Neuroflow.Networks.Neural.CPU
{
    internal sealed class PValuePropagator
    {
        #region Construct

        internal PValuePropagator(UnitIndexTable indexTable, IEnumerable<LayerForwardCompute> forwardComputes)
        {
            Contract.Requires(indexTable != null);
            Contract.Requires(forwardComputes != null);

            IndexTable = indexTable;
            ForwardComputes = forwardComputes.ToArray();
            VerifyIndexes();
        }

        [Conditional("DEBUG")]
        private void VerifyIndexes()
        {
            for (int idx = 0; idx < ForwardComputes.Length; idx++)
            {
                Debug.Assert(ForwardComputes[idx].ConnectedLayerIndex == idx);
                if (idx == ForwardComputes.Length - 1) Debug.Assert(ForwardComputes[idx].IsOutput);
            }
        } 

        #endregion

        #region Props and Fields

        internal UnitIndexTable IndexTable { get; private set; }

        internal LayerForwardCompute[] ForwardComputes { get; private set; } 

        #endregion

        #region Propagation

        internal unsafe void Propagate(float* valueBuffer, float * eVector)
        {
            Contract.Requires(valueBuffer != null);

            foreach (var comp in ForwardComputes)
            {
                int iLayerIndex = comp.ConnectedLayerIndex;
                int compOutputBufferSize = comp.OutputBuffer.Size;
                DataParallel.Do(comp.OutputBuffer.Size, comp.RunParallel, ctx =>
                {
                    for (int iValueIndex = ctx.WorkItemRange.MinValue; iValueIndex <= ctx.WorkItemRange.MaxValue; iValueIndex++)
                    {
                        // i: iLayerIndex, iValueIndex

                        // Compute for Weights:
                        for (int jLayerIndex = 0; jLayerIndex < comp.PWeightBuffers.Length; jLayerIndex++)
                        {
                            var layerRelatedPBuffs = comp.PWeightBuffers[jLayerIndex];
                            var layerRelatedPrevPBuffs = comp.PrevPWeightBuffers[jLayerIndex];
                            int inputLayerSize = comp.InputValueAccessItems[jLayerIndex].InputSize;

                            Debug.Assert(compOutputBufferSize * inputLayerSize == layerRelatedPBuffs.Length);
                            Debug.Assert(compOutputBufferSize * inputLayerSize == layerRelatedPrevPBuffs.Length);

                            for (int jValueIndex = 0; jValueIndex < inputLayerSize; jValueIndex++)
                            {
                                int ijValueIndex = WeightAccessor.GetWeightValueIndex(jValueIndex, iValueIndex, compOutputBufferSize);

                                var valueRelatedPBuffs = layerRelatedPBuffs[ijValueIndex];
                                var valueRelatedPrevPBuffs = layerRelatedPrevPBuffs[ijValueIndex];

                                // i: iLayerIndex, iValueIndex
                                // j: jLayerIndex, jValueIndex

                                FlipPWeights(comp);
                                ComputePValues(valueBuffer, comp, valueRelatedPBuffs, valueRelatedPrevPBuffs, iLayerIndex, iValueIndex, jLayerIndex, jValueIndex, eVector);
                            }
                        }

                        // Compute for Bias
                        if (comp.PBiasBuffers != null)
                        {
                            FlipPBias(comp);
                            ComputePValues(valueBuffer, comp, comp.PBiasBuffers, comp.PrevPBiasBuffers, iLayerIndex, iValueIndex, -1, -1, eVector);
                        }
                    }
                });
            }
        }

        unsafe private void ComputePValues(float* valueBuffer, LayerForwardCompute comp, IntRange[] valueRelatedPBuffs, IntRange[] valueRelatedPrevPBuffs, int iLayerIndex, int iValueIndex, int jLayerIndex, int jValueIndex, float* eVector)
        {
            int outputLayerIndex = valueRelatedPBuffs.Length - 1;
            Debug.Assert(outputLayerIndex == ForwardComputes.Length - 1);
            Debug.Assert(outputLayerIndex == ForwardComputes[ForwardComputes.Length - 1].ConnectedLayerIndex);
            for (int kLayerIndex = 0; kLayerIndex < valueRelatedPBuffs.Length; kLayerIndex++)
            {
                float gradient = 0.0f;
                bool computeGradient = kLayerIndex == outputLayerIndex && eVector != null;

                var currentPBuffValueRange = valueRelatedPBuffs[kLayerIndex];
                var currentPrevPBuffValueRange = valueRelatedPrevPBuffs[kLayerIndex];
                int currentPBuffValueRangeSize = currentPBuffValueRange.Size;
                for (int kValueIndex = 0; kValueIndex < currentPBuffValueRangeSize; kValueIndex++)
                {
                    // i: iLayerIndex, iValueIndex
                    // j: jLayerIndex, jValueIndex
                    // k: kLayerIndex, kValueIndex

                    float netDeriv_k = GetNetDerivValue(valueBuffer, kLayerIndex, kValueIndex);
                    float sum = 0.0f;

                    var comp_k = ForwardComputes[kLayerIndex];
                    foreach (var upperNonInputLayerInfo in comp_k.UpperNonInputLayerInfos)
                    {
                        int lLayerIndex = upperNonInputLayerInfo.LayerIndex;
                        var accessItem = comp_k.InputValueAccessItems[upperNonInputLayerInfo.WeightedErrorBufferIndex];
                        for (int lValueIndex = 0; lValueIndex < accessItem.InputSize; lValueIndex++)
                        {
                            // i: iLayerIndex, iValueIndex
                            // j: jLayerIndex, jValueIndex
                            // k: kLayerIndex, kValueIndex
                            // l: lLayerIndex, lValueIndex

                            float weight_k_l = valueBuffer[accessItem.WeightBufferBeginIndex + WeightAccessor.GetWeightValueIndex(lValueIndex, kValueIndex, currentPBuffValueRangeSize)];
                            float p_i_j_l = valueBuffer[valueRelatedPrevPBuffs[lLayerIndex].MinValue + lValueIndex];

                            sum += weight_k_l * p_i_j_l;
                        }
                    }

                    if (iLayerIndex == kLayerIndex && iValueIndex == kValueIndex)
                    {
                        sum += GetInputValue(valueBuffer, comp, jLayerIndex, jValueIndex);
                    }

                    if (computeGradient)
                    {
                        gradient += eVector[kValueIndex] * (valueBuffer[valueRelatedPBuffs[kLayerIndex].MinValue + kValueIndex] = netDeriv_k * sum);
                    }
                    else
                    {
                        valueBuffer[valueRelatedPBuffs[kLayerIndex].MinValue + kValueIndex] = netDeriv_k * sum;
                    }
                }

                if (computeGradient)
                {
                    //Console.WriteLine(gradient);
                    SetGradientValue(valueBuffer, comp, iValueIndex, jLayerIndex, jValueIndex, gradient);
                }
            }
        }

        private unsafe void SetGradientValue(float* valueBuffer, LayerForwardCompute comp, int iValueIndex, int jLayerIndex, int jValueIndex, float gradient)
        {
            if (jLayerIndex == -1)
            {
                // Bias
                Debug.Assert(jValueIndex == -1);
                Debug.Assert(comp.BiasGradientValueIndex != null);
                valueBuffer[comp.BiasGradientValueIndex.Value] = gradient;
                valueBuffer[comp.BiasGradientSumValueIndex.Value] += gradient;
            }
            else
            {
                int wvIndex = WeightAccessor.GetWeightValueIndex(jValueIndex, iValueIndex, comp.OutputBuffer.Size);
                valueBuffer[comp.GradientBuffers[jLayerIndex].MinValue + wvIndex] = gradient;
                valueBuffer[comp.GradientSumBuffers[jLayerIndex].MinValue + wvIndex] += gradient;
            }
        }

        private unsafe float GetInputValue(float* valueBuffer, LayerForwardCompute comp, int jLayerIndex, int jValueIndex)
        {
            if (jLayerIndex == -1)
            {
                // Bias
                Debug.Assert(jValueIndex == -1);
                Debug.Assert(comp.BiasValueIndex != null);
                return valueBuffer[comp.BiasValueIndex.Value];
            }

            var accessItem = comp.InputValueAccessItems[jLayerIndex];
            var valueIndex = accessItem.InputBufferBeginIndex + jValueIndex;
            Debug.Assert(valueIndex <= accessItem.InputBufferBeginIndex + accessItem.InputSize);
            return valueBuffer[valueIndex];
        }

        unsafe private float GetNetDerivValue(float* valueBuffer, int kLayerIndex, int kValueIndex)
        {
            Debug.Assert(ForwardComputes[kLayerIndex].NetDerivBuffer != null);
            Debug.Assert(ForwardComputes[kLayerIndex].NetDerivBuffer.Value.MinValue + kValueIndex <= ForwardComputes[kLayerIndex].NetDerivBuffer.Value.MaxValue); 

            return valueBuffer[ForwardComputes[kLayerIndex].NetDerivBuffer.Value.MinValue + kValueIndex];
        }

        private void FlipPWeights(LayerForwardCompute comp)
        {
            var temp = comp.PWeightBuffers;
            comp.PWeightBuffers = comp.PrevPWeightBuffers;
            comp.PrevPWeightBuffers = temp;
        }

        private void FlipPBias(LayerForwardCompute comp)
        {
            var temp = comp.PBiasBuffers;
            comp.PBiasBuffers = comp.PrevPBiasBuffers;
            comp.PrevPBiasBuffers = temp;
        }

        #endregion
    }
}
