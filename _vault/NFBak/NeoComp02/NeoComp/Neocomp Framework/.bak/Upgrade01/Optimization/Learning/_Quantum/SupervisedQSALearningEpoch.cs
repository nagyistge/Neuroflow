using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks.Neural;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.Learning
{
    public sealed class SupervisedQSALearningEpoch : SupervisedAdjusterLearningEpoch<SupervisedQSALearningStrategy>
    {
        #region Constructors

        public SupervisedQSALearningEpoch(
            SupervisedQSALearningStrategy strategy, 
            NeuralNetworkTest test = null,
            int? monteCarloSelect = null,
            bool preserveTestOrder = false)
            : base(strategy, LearningMode.Batch, test, monteCarloSelect, preserveTestOrder)
        {
            Contract.Requires(strategy != null);
            Contract.Requires(!monteCarloSelect.HasValue || monteCarloSelect.Value > 1);
        }

        public SupervisedQSALearningEpoch(
            NeuralNetwork network,
            NeuralNetworkTest test = null,
            int? monteCarloSelect = null,
            bool preserveTestOrder = false)
            : base(new SupervisedQSALearningStrategy(network), LearningMode.Batch, test, monteCarloSelect, preserveTestOrder)
        {
            Contract.Requires(network != null);
            Contract.Requires(!monteCarloSelect.HasValue || monteCarloSelect.Value > 1);
        }

        #endregion
    }
}
