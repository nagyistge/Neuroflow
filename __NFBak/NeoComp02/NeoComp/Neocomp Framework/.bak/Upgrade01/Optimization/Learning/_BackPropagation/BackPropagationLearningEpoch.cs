using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks.Neural;
using NeoComp.Core;
using System.Diagnostics.Contracts;
using NeoComp.Computations;

namespace NeoComp.Optimization.Learning
{
    public sealed class BackPropagationLearningEpoch : SupervisedAdjusterLearningEpoch<BackPropagationLearningStrategy>
    {
        #region Constructors

        public BackPropagationLearningEpoch(BackPropagationLearningStrategy strategy, NeuralNetworkTest test = null, bool preserveTestOrder = false)
            : base(strategy, LearningMode.Online, test, null, preserveTestOrder)
        {
            Contract.Requires(strategy != null);
        }

        public BackPropagationLearningEpoch(NeuralNetwork network, NeuralNetworkTest test = null, bool preserveTestOrder = false)
            : base(new BackPropagationLearningStrategy(network), LearningMode.Online, test, null, preserveTestOrder)
        {
            Contract.Requires(network != null);
        }

        #endregion
    }
}
