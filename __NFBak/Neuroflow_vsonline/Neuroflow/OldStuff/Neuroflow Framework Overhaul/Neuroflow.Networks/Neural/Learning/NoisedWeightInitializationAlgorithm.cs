using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core;

namespace Neuroflow.Networks.Neural.Learning
{
    public sealed class NoisedWeightInitializationAlgorithm : ForwardLearningAlgorithm<NoisedWeightInitializationRule>
    {
        protected override unsafe void InitializeNewTraining(double* values, InputValueIndexes* inputValueIndexes, OutputValueIndexes* outputValueIndexes)
        {
            int count = ValueCount;
            var rule = Rule;
            for (int i = 0; i < count; i++)
            {
                values[inputValueIndexes[i].WeightValueIndex] = (rule.Noise * RandomGenerator.Random.NextDouble()) * 2.0 - rule.Noise;
            }
        }
    }
}
