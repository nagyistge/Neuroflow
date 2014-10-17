using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core;
using System.Diagnostics;
using Neuroflow.Core.Vectors;

namespace Neuroflow.Networks.Neural.Learning.Managed
{
    public abstract class AutoAdjustedGradientDescentAlgorithm<T> : GradientDescentAlgorithm<T>
        where T : AutoAdjustedGradientDescentRule
    {
        #region Fields

        WeightRelatedValues stepSizes, lastGradients;

        bool begin = true;

        #endregion

        #region Init

        protected override void Ininitalize(BufferAllocator allocator)
        {
            base.Ininitalize(allocator);
            stepSizes = new WeightRelatedValues(allocator, ConnectedLayers);
            lastGradients = new WeightRelatedValues(allocator, ConnectedLayers);
        }

        protected internal override unsafe void InitializeNewRun(float* valueBuffer)
        {
            base.InitializeNewRun(valueBuffer);
            stepSizes.Fill(valueBuffer, (float)Rule.StepSize);
            lastGradients.Zero(valueBuffer);
            begin = true;
        }

        #endregion

        #region Iterations

        protected override unsafe void BatchBackwardIteration(float* valueBuffer, int batchSize, double averageError)
        {
            BatchStepAdatpiveState(valueBuffer, batchSize);
            DoBatchWeightUpdate(valueBuffer, batchSize, stepSizes);
        }

        protected override unsafe void StochasticBackwardIteration(float* valueBuffer, double averageError)
        {
            if (Rule.StochasticAdaptiveStateUpdate) StochasticStepAdatpiveState(valueBuffer);
            DoStochasticWeightUpdate(valueBuffer, stepSizes);
        }

        protected override unsafe void StochasticEndOfBatchBackwardIteration(float* valueBuffer, int batchSize, double averageError)
        {
            if (!Rule.StochasticAdaptiveStateUpdate) BatchStepAdatpiveState(valueBuffer, batchSize);
        }

        #endregion

        #region Step State

        private unsafe void BatchStepAdatpiveState(float* valueBuffer, int batchSize)
        {
            float fBatchSize = batchSize;
            if (begin)
            {
                for (int layerIndex = 0; layerIndex < ConnectedLayers.Length; layerIndex++)
                {
                    var layer = ConnectedLayers[layerIndex];
                    fixed (IntRange* pGradSumBuff = layer.GradientSumBuffers)
                    {
                        for (int buffIndex = 0; buffIndex < layer.GradientSumBuffers.Length; buffIndex++)
                        {
                            var gradSumBuff = pGradSumBuff[buffIndex];
                            int gradSumBuffSize = gradSumBuff.Size;
                            var lastGradBuff = lastGradients.GetBuffer(layerIndex, buffIndex);
                            for (int weightIndex = 0; weightIndex < gradSumBuffSize; weightIndex++)
                            {
                                int lastGradIndex = lastGradBuff.MinValue + weightIndex;
                                int gradSumIndex = gradSumBuff.MinValue + weightIndex;

                                Debug.Assert(lastGradIndex <= lastGradBuff.MaxValue);
                                Debug.Assert(gradSumIndex <= gradSumBuff.MaxValue);

                                valueBuffer[lastGradIndex] = valueBuffer[gradSumIndex] / fBatchSize;
                            }
                        }
                    }
                }
                begin = false;
            }
            else
            {
                var rule = Rule;
                for (int layerIndex = 0; layerIndex < ConnectedLayers.Length; layerIndex++)
                {
                    var layer = ConnectedLayers[layerIndex];
                    fixed (IntRange* pGradSumBuff = layer.GradientSumBuffers)
                    {
                        for (int buffIndex = 0; buffIndex < layer.GradientSumBuffers.Length; buffIndex++)
                        {
                            var gradSumBuff = pGradSumBuff[buffIndex];
                            var lastGradBuff = lastGradients.GetBuffer(layerIndex, buffIndex);
                            var stepSizeBuff = stepSizes.GetBuffer(layerIndex, buffIndex);

                            for (int weightIndex = 0; weightIndex < gradSumBuff.Size; weightIndex++)
                            {
                                int lastGradIndex = lastGradBuff.MinValue + weightIndex;
                                int stepSizeIndex = stepSizeBuff.MinValue + weightIndex;
                                int gradSumIndex = gradSumBuff.MinValue + weightIndex;

                                Debug.Assert(lastGradIndex <= lastGradBuff.MaxValue);
                                Debug.Assert(stepSizeIndex <= stepSizeBuff.MaxValue);
                                Debug.Assert(gradSumIndex <= gradSumBuff.MaxValue);

                                float lastGradient = valueBuffer[lastGradIndex];
                                float currentStepSize = valueBuffer[stepSizeIndex];
                                float currentGradient = valueBuffer[gradSumIndex] / fBatchSize;

                                valueBuffer[lastGradIndex] = currentGradient;
                                valueBuffer[stepSizeIndex] = (float)rule.StepSizeRange.Cut(CalculateStepSize(currentStepSize, lastGradient, currentGradient));
                            }
                        }
                    }
                }
            }
        }

        private unsafe void StochasticStepAdatpiveState(float* valueBuffer)
        {
            if (begin)
            {
                for (int layerIndex = 0; layerIndex < ConnectedLayers.Length; layerIndex++)
                {
                    var layer = ConnectedLayers[layerIndex];
                    fixed (IntRange* pGradBuff = layer.GradientBuffers)
                    {
                        for (int buffIndex = 0; buffIndex < layer.GradientBuffers.Length; buffIndex++)
                        {
                            var gradBuff = pGradBuff[buffIndex];
                            int gradBuffSize = gradBuff.Size;
                            var lastGradBuff = lastGradients.GetBuffer(layerIndex, buffIndex);
                            for (int weightIndex = 0; weightIndex < gradBuffSize; weightIndex++)
                            {
                                int lastGradIndex = lastGradBuff.MinValue + weightIndex;
                                int gradIndex = gradBuff.MinValue + weightIndex;

                                Debug.Assert(lastGradIndex <= lastGradBuff.MaxValue);
                                Debug.Assert(gradIndex <= gradBuff.MaxValue);

                                valueBuffer[lastGradIndex] = valueBuffer[gradIndex];
                            }
                        }
                    }
                }
                begin = false;
            }
            else
            {
                var rule = Rule;
                for (int layerIndex = 0; layerIndex < ConnectedLayers.Length; layerIndex++)
                {
                    var layer = ConnectedLayers[layerIndex];
                    fixed (IntRange* pGradBuff = layer.GradientBuffers)
                    {
                        for (int buffIndex = 0; buffIndex < layer.GradientBuffers.Length; buffIndex++)
                        {
                            var gradBuff = pGradBuff[buffIndex];
                            var lastGradBuff = lastGradients.GetBuffer(layerIndex, buffIndex);
                            var stepSizeBuff = stepSizes.GetBuffer(layerIndex, buffIndex);

                            for (int weightIndex = 0; weightIndex < gradBuff.Size; weightIndex++)
                            {
                                int lastGradIndex = lastGradBuff.MinValue + weightIndex;
                                int stepSizeIndex = stepSizeBuff.MinValue + weightIndex;
                                int gradIndex = gradBuff.MinValue + weightIndex;

                                Debug.Assert(lastGradIndex <= lastGradBuff.MaxValue);
                                Debug.Assert(gradIndex <= gradBuff.MaxValue);
                                Debug.Assert(stepSizeIndex <= stepSizeBuff.MaxValue);

                                float lastGradient = valueBuffer[lastGradIndex];
                                float currentGradient = valueBuffer[gradIndex];
                                float currentStepSize = valueBuffer[stepSizeIndex];

                                valueBuffer[lastGradIndex] = currentGradient;
                                valueBuffer[stepSizeIndex] = (float)rule.StepSizeRange.Cut(CalculateStepSize(currentStepSize, lastGradient, currentGradient));
                            }
                        }
                    }
                }
            }
        }

        protected abstract float CalculateStepSize(float currentStepSize, float lastGradient, float currentGradient);

        #endregion
    }
}
