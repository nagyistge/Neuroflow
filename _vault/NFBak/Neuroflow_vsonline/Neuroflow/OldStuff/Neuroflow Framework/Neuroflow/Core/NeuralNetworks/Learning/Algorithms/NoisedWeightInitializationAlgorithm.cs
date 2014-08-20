﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core.NeuralNetworks;
using Neuroflow.Core;

namespace Neuroflow.Core.NeuralNetworks.Learning.Algorithms
{
    public sealed class NoisedWeightInitializationAlgorithm : ForwardLearningAlgorithm
    {
        protected internal override void InitializeNewRun()
        {
            foreach (var lc in LearningConnections)
            {
                var rule = (NoisedWeightInitializationRule)lc.Rule;
                lc.Connection.Weight = (rule.Noise * RandomGenerator.Random.NextDouble()) * 2.0 - rule.Noise;
            }
        }

        protected internal override void ForwardIteration(bool isNewBatch)
        {
            // We are done.
        }
    }
}
