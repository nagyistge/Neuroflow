using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core;
using Neuroflow.Core.Vectors;

namespace Neuroflow.Networks.Neural.Learning.Managed
{
    public sealed class QSAAlgorithm : ErrorBasedLearningAlgorithm<QSARule>
    {
        #region Fields

        WeightRelatedValues means, stdDevs;

        double lastError;

        #endregion

        #region Init

        protected override void Ininitalize(BufferAllocator allocator)
        {
            means = new WeightRelatedValues(allocator, ConnectedLayers);
            stdDevs = new WeightRelatedValues(allocator, ConnectedLayers);
        }

        protected internal override unsafe void InitializeNewRun(float* valueBuffer)
        {
            means.Fill(valueBuffer, 0.5f);
            stdDevs.Fill(valueBuffer, 0.5f);
            lastError = double.MaxValue;
            SetWeights(valueBuffer);
        }

        #endregion

        #region Run

        protected override unsafe void BatchBackwardIteration(float* valueBuffer, int batchSize, double averageError)
        {
            Run(valueBuffer, averageError);
        }

        private unsafe void Run(float* valueBuffer, double averageError)
        {
            if (lastError != double.MaxValue)
            {
                if (averageError <= lastError)
                {
                    Stabilize(valueBuffer);
                }
                else if (averageError > lastError)
                {
                    Dissolve(valueBuffer);
                }
            }
            SetWeights(valueBuffer);
            lastError = averageError;
        }
        
        #endregion

        #region Stabilize

        private unsafe void Stabilize(float* valueBuffer)
        {
            for (int layerIndex = 0; layerIndex < ConnectedLayers.Length; layerIndex++)
            {
                var layer = ConnectedLayers[layerIndex];
                for (int buffIndex = 0; buffIndex < layer.WeightedInputBuffers.Length; buffIndex++)
                {
                    var weightBuff = layer.WeightedInputBuffers[buffIndex].WeightBuffer;
                    var meanBuff = means.GetBuffer(layerIndex, buffIndex);
                    var stdDevBuff = means.GetBuffer(layerIndex, buffIndex);

                    for (int weightIndex = 0; weightIndex < weightBuff.Size; weightIndex++)
                    {
                        float mean = valueBuffer[meanBuff.MinValue + weightIndex];
                        float stdDev = valueBuffer[stdDevBuff.MinValue + weightIndex];
                        float weight = valueBuffer[weightBuff.MinValue + weightIndex];

                        float movement = Follow(ref mean, weight, (float)Rule.Up);
                        stdDev = Shrink(stdDev, movement);

                        valueBuffer[meanBuff.MinValue + weightIndex] = mean;
                        valueBuffer[stdDevBuff.MinValue + weightIndex] = stdDev;
                    }
                }
            }
        }

        #endregion

        #region Dissolve

        private unsafe void Dissolve(float* valueBuffer)
        {
            for (int layerIndex = 0; layerIndex < ConnectedLayers.Length; layerIndex++)
            {
                var layer = ConnectedLayers[layerIndex];
                for (int buffIndex = 0; buffIndex < layer.WeightedInputBuffers.Length; buffIndex++)
                {
                    var weightBuff = layer.WeightedInputBuffers[buffIndex].WeightBuffer;
                    var meanBuff = means.GetBuffer(layerIndex, buffIndex);
                    var stdDevBuff = means.GetBuffer(layerIndex, buffIndex);

                    for (int weightIndex = 0; weightIndex < weightBuff.Size; weightIndex++)
                    {
                        float mean = valueBuffer[meanBuff.MinValue + weightIndex];
                        float stdDev = valueBuffer[stdDevBuff.MinValue + weightIndex];
                        float weight = valueBuffer[weightBuff.MinValue + weightIndex];

                        if (stdDev < 0.5f)
                        {
                            stdDev = Grow(stdDev, (0.5f - stdDev) * (float)Rule.Down);
                        }

                        float stdDev2 = stdDev / 2.0f;
                        if ((mean + stdDev2) > 1.0f || (mean - stdDev2) < 0.0f)
                        {
                            Follow(ref mean, 0.5f, (float)Rule.Down);
                        }

                        valueBuffer[meanBuff.MinValue + weightIndex] = mean;
                        valueBuffer[stdDevBuff.MinValue + weightIndex] = stdDev;
                    }
                }
            }
        }

        #endregion

        #region Calculations

        private float Follow(ref float mean, float weight, float rate)
        {
            float movement = 0.0f;
            if (weight < mean)
            {
                movement = (mean - weight) * rate;
                mean -= movement;
            }
            else if (weight > mean)
            {
                movement = (weight - mean) * rate;
                mean += movement;
            }
            return movement == 0.0f ? float.Epsilon : movement;
        }

        private float Shrink(float stdDev, float by)
        {
            stdDev -= stdDev * by * 0.01f;
            if (stdDev < 0.0f) stdDev = 0.0f;
            return stdDev;
        }

        private float Grow(float stdDev, float by)
        {
            stdDev += stdDev * by * 0.01f;
            if (stdDev > 0.5f) stdDev = 0.5f;
            return stdDev;
        }

        #endregion

        #region Set Weights

        private unsafe void SetWeights(float* valueBuffer)
        {
            for (int layerIndex = 0; layerIndex < ConnectedLayers.Length; layerIndex++)
            {
                var layer = ConnectedLayers[layerIndex];
                for (int buffIndex = 0; buffIndex < layer.WeightedInputBuffers.Length; buffIndex++)
                {
                    var weightBuff = layer.WeightedInputBuffers[buffIndex].WeightBuffer;
                    var meanBuff = means.GetBuffer(layerIndex, buffIndex);
                    var stdDevBuff = means.GetBuffer(layerIndex, buffIndex);

                    for (int weightIndex = 0; weightIndex < weightBuff.Size; weightIndex++)
                    {
                        float mean = valueBuffer[meanBuff.MinValue + weightIndex];
                        float stdDev = valueBuffer[stdDevBuff.MinValue + weightIndex];
                        valueBuffer[weightBuff.MinValue + weightIndex] = CalculateWeight(mean, stdDev);
                    }
                }
            }
        }

        private unsafe float CalculateWeight(float mean, float stdDev)
        {
            float normal;
            if (Rule.DistributionType == DistributionType.Uniform)
            {
                float min = mean - stdDev;
                float max = mean + stdDev;
                float d = max - min;
                normal = (float)RandomGenerator.Random.NextDouble() * d + min;
            }
            else
            {
                normal = (float)Statistics.GenerateGauss(mean, stdDev);
            }
            return (normal * 2.0f) - 1.0f;
        }

        #endregion
    }
}
