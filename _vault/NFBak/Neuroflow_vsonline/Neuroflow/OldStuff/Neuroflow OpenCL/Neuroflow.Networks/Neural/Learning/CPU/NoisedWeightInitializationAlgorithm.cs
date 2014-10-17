using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core;

namespace Neuroflow.Networks.Neural.Learning.CPU
{
    public sealed class NoisedWeightInitializationAlgorithm : LearningAlgorithm
    {
        protected internal override unsafe void InitializeNewRun(float* valueBuffer)
        {
            var rule = (NoisedWeightInitializationRule)Rule;
            foreach (var cl in ConnectedLayers)
            {
                DataParallel.Do(cl.WeightedInputBuffers.Length, RunParallel, (ctx) =>
                {
                    fixed (WeightedValueBuffer* buffs = cl.WeightedInputBuffers)
                    {
                        for (int buffIndex = ctx.WorkItemRange.MinValue; buffIndex <= ctx.WorkItemRange.MaxValue; buffIndex++)
                        {
                            var weightBuff = buffs[buffIndex].WeightBuffer;
                            for (int weightIndex = weightBuff.MinValue; weightIndex <= weightBuff.MaxValue; weightIndex++)
                            {
                                valueBuffer[weightIndex] = (float)((rule.Noise * (float)RandomGenerator.Random.NextDouble()) * 2.0 - rule.Noise);
                            }
                        }
                    }
                });
            }
        }
    }
}
