using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core;
using System.Diagnostics;

namespace Neuroflow.Networks.Neural.Learning.CPU
{
    public sealed class AlopexBAlgorithm : ErrorBasedLearningAlgorithm<AlopexBRule>
    {
        #region Fields

        int iterationNo;

        double lastError;

        double lastDeltaError;

        double cDiv;

        WeightRelatedValues tMinusOneWeights;

        WeightRelatedValues tWeights;

        #endregion

        #region Init

        protected override void Ininitalize(BufferAllocator allocator)
        {
            tWeights = new WeightRelatedValues(allocator, ConnectedLayers);
            tMinusOneWeights = new WeightRelatedValues(allocator, ConnectedLayers);
        }

        protected internal override unsafe void InitializeNewRun(float* valueBuffer)
        {
            iterationNo = 0;
            lastError = 0.0;
            lastDeltaError = 0.0;
            cDiv = 0.0;
            tWeights.Zero(valueBuffer);
        }

        #endregion

        #region Run

        protected override unsafe void BatchBackwardIteration(float* valueBuffer, int batchSize, double averageError)
        {
            Iterate(valueBuffer, (float)averageError);
        }

        unsafe private void Iterate(float* valueBuffer, float averageError)
        {
            if (++iterationNo > 1)
            {
                Step(valueBuffer, averageError);
            }

            lastDeltaError = averageError - lastError;
            lastError = averageError;
            tMinusOneWeights.CopyFrom(valueBuffer, tWeights);
            tWeights.SaveWeights(valueBuffer);
        }

        private unsafe void Step(float* valueBuffer, float averageError)
        {
            double deltaError = averageError - lastError;

            double cDivCurrent = (Rule.Lambda * Math.Pow(Rule.Lambda - 1.0, iterationNo - 2)) * Math.Abs(lastDeltaError);
            if ((cDiv += cDivCurrent) == 0.0) return;
            
            for (int layerIndex = 0; layerIndex < ConnectedLayers.Length; layerIndex++)
            {
                var layer = ConnectedLayers[layerIndex];
                fixed (WeightedValueBuffer* pWeightedInBuffers = layer.WeightedInputBuffers)
                {
                    for (int buffIndex = 0; buffIndex < layer.WeightedInputBuffers.Length; buffIndex++)
                    {
                        var weightBuff = pWeightedInBuffers[buffIndex].WeightBuffer;
                        var lastWeightBuff = tMinusOneWeights.GetBuffer(layerIndex, buffIndex);

                        DataParallel.Do(weightBuff.Size, RunParallel, ctx =>
                        {
                            for (int weightIndex = ctx.WorkItemRange.MinValue; weightIndex <= ctx.WorkItemRange.MaxValue; weightIndex++)
                            {
                                int weightValueIndex = weightBuff.MinValue + weightIndex;
                                int lastWeightValueIndex = lastWeightBuff.MinValue + weightIndex;

                                double deltaWeight = valueBuffer[weightValueIndex] - valueBuffer[lastWeightValueIndex];

                                double c = ((double)Math.Sign(deltaWeight) * deltaError) / cDiv;
                                double p = LogisticFunction(c);
                                double ksi = (double)Math.Sign(RandomGenerator.Random.NextDouble() - p);

                                valueBuffer[weightValueIndex] = (float)(valueBuffer[weightValueIndex] - (Rule.Eta * deltaWeight * deltaError) + (Rule.Gamma * ksi));
                            }
                        });
                    }
                }
            }
        }

        static double LogisticFunction(double val)
        {
            return (1.0 / (1.0 + Math.Exp(-val)));
        } 

        #endregion
    }
}