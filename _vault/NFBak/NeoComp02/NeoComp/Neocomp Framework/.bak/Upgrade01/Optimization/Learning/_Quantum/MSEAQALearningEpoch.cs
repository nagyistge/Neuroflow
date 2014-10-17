using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Networks.Neural;

namespace NeoComp.Optimization.Learning
{
    public sealed class MSEAQALearningEpoch : SupervisedAdjusterLearningEpoch<MSEAQALearningStrategy>
    {
        public MSEAQALearningEpoch(
            MSEAQALearningStrategy strategy,
            NeuralNetworkTest test = null,
            int? monteCarloSelect = null,
            bool preserveTestOrder = false)
            : base(strategy, LearningMode.Batch, test, monteCarloSelect, preserveTestOrder)
        {
            Contract.Requires(strategy != null);
        }

        public MSEAQALearningEpoch(
            NeuralNetwork network,
            NeuralNetworkTest test = null,
            int? monteCarloSelect = null,
            bool preserveTestOrder = false)
            : base(new MSEAQALearningStrategy(network), LearningMode.Parallel, test, monteCarloSelect, preserveTestOrder)
        {
            Contract.Requires(network != null);
        }
    }
}
