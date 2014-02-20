using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using Neuroflow.Networks.Neural;
using Neuroflow.Core.Vectors;

namespace Neuroflow.Networks.Neural.Learning
{
    public sealed class GradientDescentAlgorithm : GradientDescentAlgorithm<GradientDescentRule> { }
    
    public abstract class GradientDescentAlgorithm<T> : ErrorBasedLearningAlgorithm<T>
        where T : GradientDescentRule
    {
        #region Fields

        WeightRelatedValues deltaValues; 

        #endregion

        #region Init

        protected override void Ininitalize(BufferAllocator allocator)
        {
            deltaValues = new WeightRelatedValues(allocator, ConnectedLayers);
        }

        protected internal override unsafe void InitializeNewRun(float* valueBuffer)
        {
            deltaValues.Zero(valueBuffer);
        }

        #endregion

        #region Learn

        #region Batch

        protected override unsafe void BatchBackwardIteration(float* valueBuffer, int batchSize, double averageError)
        {
            float momentum = (float)Rule.Momentum;
            float stepSize = (float)Rule.StepSize;
            float fBatchSize = batchSize;
            for (int layerIndex = 0; layerIndex < ConnectedLayers.Length; layerIndex++)
            {
                var layer = ConnectedLayers[layerIndex];
                fixed (WeightedValueBuffer* pWeightedInBuffers = layer.WeightedInputBuffers)
                fixed (IntRange* pGradSumBuffers = layer.GradientSumBuffers)
                {
                    Debug.Assert(layer.WeightedInputBuffers.Length == layer.GradientSumBuffers.Length);

                    for (int buffIndex = 0; buffIndex < layer.WeightedInputBuffers.Length; buffIndex++)
                    {
                        var weightBuff = pWeightedInBuffers[buffIndex].WeightBuffer;
                        var gradSumBuff = pGradSumBuffers[buffIndex];
                        var deltaBuff = deltaValues.GetBuffer(layerIndex, buffIndex);

                        for (int weightIndex = 0; weightIndex < weightBuff.Size; weightIndex++)
                        {
                            int deltaIndex = deltaBuff.MinValue + weightIndex;
                            int gradSumIndex = gradSumBuff.MinValue + weightIndex;
                            int weightValueIndex = weightBuff.MinValue + weightIndex;

                            Debug.Assert(deltaIndex <= deltaBuff.MaxValue);
                            Debug.Assert(gradSumIndex <= gradSumBuff.MaxValue);
                            Debug.Assert(weightValueIndex <= weightBuff.MaxValue);

                            float update = (momentum * valueBuffer[deltaIndex]) + (valueBuffer[gradSumIndex] / fBatchSize) * stepSize;
                            valueBuffer[weightValueIndex] += update;
                            valueBuffer[deltaIndex] = update;
                        }
                    }
                }
            }
        }

        protected unsafe void DoBatchWeightUpdate(float* valueBuffer, int batchSize, WeightRelatedValues stepSizes)
        {
            float momentum = (float)Rule.Momentum;
            float fBatchSize = batchSize;
            for (int layerIndex = 0; layerIndex < ConnectedLayers.Length; layerIndex++)
            {
                var layer = ConnectedLayers[layerIndex];
                fixed (WeightedValueBuffer* pWeightedInBuffers = layer.WeightedInputBuffers)
                fixed (IntRange* pGradSumBuffers = layer.GradientSumBuffers)
                {
                    Debug.Assert(layer.WeightedInputBuffers.Length == layer.GradientSumBuffers.Length);

                    for (int buffIndex = 0; buffIndex < layer.WeightedInputBuffers.Length; buffIndex++) // TODO: Parallelize
                    {
                        var weightBuff = pWeightedInBuffers[buffIndex].WeightBuffer;
                        var gradSumBuff = pGradSumBuffers[buffIndex];
                        var deltaBuff = deltaValues.GetBuffer(layerIndex, buffIndex);
                        var stepSizeBuff = stepSizes.GetBuffer(layerIndex, buffIndex);

                        for (int weightIndex = 0; weightIndex < weightBuff.Size; weightIndex++)
                        {
                            int deltaIndex = deltaBuff.MinValue + weightIndex;
                            int stepSizeIndex = stepSizeBuff.MinValue + weightIndex;
                            int gradSumIndex = gradSumBuff.MinValue + weightIndex;
                            int weightValueIndex = weightBuff.MinValue + weightIndex;

                            Debug.Assert(deltaIndex <= deltaBuff.MaxValue);
                            Debug.Assert(stepSizeIndex <= stepSizeBuff.MaxValue);
                            Debug.Assert(gradSumIndex <= gradSumBuff.MaxValue);
                            Debug.Assert(weightValueIndex <= weightBuff.MaxValue);

                            float stepSize = valueBuffer[stepSizeIndex];
                            float update = (momentum * valueBuffer[deltaIndex]) + (valueBuffer[gradSumIndex] / fBatchSize) * stepSize;
                            valueBuffer[weightValueIndex] += update;
                            valueBuffer[deltaIndex] = update;
                        }
                    }
                }
            }
        } 

        #endregion

        #region Stochastic

        protected override unsafe void StochasticBackwardIteration(float* valueBuffer, double averageError)
        {
            float momentum = (float)Rule.Momentum;
            float stepSize = (float)Rule.StepSize;
            for (int layerIndex = 0; layerIndex < ConnectedLayers.Length; layerIndex++)
            {
                var layer = ConnectedLayers[layerIndex];
                fixed (WeightedValueBuffer* pWeightedInBuffers = layer.WeightedInputBuffers)
                fixed (IntRange* pGradBuffers = layer.GradientBuffers)
                {
                    Debug.Assert(layer.WeightedInputBuffers.Length == layer.GradientSumBuffers.Length);

                    for (int buffIndex = 0; buffIndex < layer.WeightedInputBuffers.Length; buffIndex++)
                    {
                        var weightBuff = pWeightedInBuffers[buffIndex].WeightBuffer;
                        var gradBuff = pGradBuffers[buffIndex];
                        var deltaBuff = deltaValues.GetBuffer(layerIndex, buffIndex);

                        for (int weightIndex = 0; weightIndex < weightBuff.Size; weightIndex++)
                        {
                            int deltaIndex = deltaBuff.MinValue + weightIndex;
                            int gradIndex = gradBuff.MinValue + weightIndex;
                            int weightValueIndex = weightBuff.MinValue + weightIndex;

                            Debug.Assert(deltaIndex <= deltaBuff.MaxValue);
                            Debug.Assert(gradIndex <= gradBuff.MaxValue);
                            Debug.Assert(weightValueIndex <= weightBuff.MaxValue);

                            float update = (momentum * valueBuffer[deltaIndex]) + valueBuffer[gradIndex] * stepSize;
                            valueBuffer[weightValueIndex] += update;
                            valueBuffer[deltaIndex] = update;
                        }
                    }
                }
            }
        }

        protected unsafe void DoStochasticWeightUpdate(float* valueBuffer, WeightRelatedValues stepSizes)
        {
            float momentum = (float)Rule.Momentum;
            for (int layerIndex = 0; layerIndex < ConnectedLayers.Length; layerIndex++)
            {
                var layer = ConnectedLayers[layerIndex];
                fixed (WeightedValueBuffer* pWeightedInBuffers = layer.WeightedInputBuffers)
                fixed (IntRange* pGradBuffers = layer.GradientBuffers)
                {
                    Debug.Assert(layer.WeightedInputBuffers.Length == layer.GradientSumBuffers.Length);

                    for (int buffIndex = 0; buffIndex < layer.WeightedInputBuffers.Length; buffIndex++) 
                    {
                        var weightBuff = pWeightedInBuffers[buffIndex].WeightBuffer;
                        var gradBuff = pGradBuffers[buffIndex];
                        var deltaBuff = deltaValues.GetBuffer(layerIndex, buffIndex);
                        var stepSizeBuff = stepSizes.GetBuffer(layerIndex, buffIndex);

                        for (int weightIndex = 0; weightIndex < weightBuff.Size; weightIndex++)
                        {
                            int deltaIndex = deltaBuff.MinValue + weightIndex;
                            int gradIndex = gradBuff.MinValue + weightIndex;
                            int weightValueIndex = weightBuff.MinValue + weightIndex;
                            int stepSizeIndex = stepSizeBuff.MinValue + weightIndex;

                            Debug.Assert(deltaIndex <= deltaBuff.MaxValue);
                            Debug.Assert(gradIndex <= gradBuff.MaxValue);
                            Debug.Assert(weightValueIndex <= weightBuff.MaxValue);
                            Debug.Assert(stepSizeIndex <= stepSizeBuff.MaxValue);

                            float stepSize = valueBuffer[stepSizeIndex];
                            float update = (momentum * valueBuffer[deltaIndex]) + valueBuffer[gradIndex] * stepSize;
                            valueBuffer[weightValueIndex] += update;
                            valueBuffer[deltaIndex] = update;
                        }
                    }
                }
            }
        }

        #endregion

        #endregion
    }
}
