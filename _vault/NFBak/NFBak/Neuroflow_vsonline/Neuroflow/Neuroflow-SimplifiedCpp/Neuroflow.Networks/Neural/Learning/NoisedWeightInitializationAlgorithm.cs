using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core;

namespace Neuroflow.Networks.Neural.Learning
{
    public sealed class NoisedWeightInitializationAlgorithm : LearningAlgorithm
    {
        protected internal override unsafe void InitializeNewRun(float* valueBuffer)
        {
            var rule = (NoisedWeightInitializationRule)Rule;
            foreach (var cl in ConnectedLayers)
            {
                fixed (WeightedValueBuffer* buffs = cl.WeightedInputBuffers)
                {
                    for (int buffIndex = 0; buffIndex < cl.WeightedInputBuffers.Length; buffIndex++)
                    {
                        var weightBuff = buffs[buffIndex].WeightBuffer;
                        for (int weightIndex = weightBuff.MinValue; weightIndex <= weightBuff.MaxValue; weightIndex++)
                        {
                            valueBuffer[weightIndex] = (float)((rule.Noise * (float)RandomGenerator.Random.NextDouble()) * 2.0 - rule.Noise);
                        }
                    }
                }
            }
        }
    }
}
