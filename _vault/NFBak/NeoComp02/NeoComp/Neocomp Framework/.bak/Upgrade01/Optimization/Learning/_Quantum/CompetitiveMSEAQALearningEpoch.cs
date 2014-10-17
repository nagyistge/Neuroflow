using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks.Neural;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.Learning
{
    public sealed class CompetitiveMSEAQALearningEpoch : SupervisedAdjusterLearningEpoch<CompetitiveMSEAQALearningStrategy>
    {
        public CompetitiveMSEAQALearningEpoch(
            CompetitiveMSEAQALearningStrategy strategy,
            NeuralNetworkTest test = null,
            int? monteCarloSelect = null,
            bool preserveTestOrder = false)
            : base(strategy, LearningMode.Parallel, test, monteCarloSelect, preserveTestOrder)
        {
            Contract.Requires(strategy != null);
        }
    }
}
