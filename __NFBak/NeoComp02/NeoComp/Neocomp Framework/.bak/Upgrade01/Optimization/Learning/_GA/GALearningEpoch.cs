using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks.Neural;
using NeoComp.Optimization.GeneticNetworks;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.Learning
{
    public sealed class GALearningEpoch : SupervisedAdjusterLearningEpoch<GALearningStrategy>
    {
        #region Constructors

        public GALearningEpoch(
            NeuralNetwork network,
            NeuralNetworkTest test = null,
            AdjustedNeuralNetworkPopulationFactory populationFactory = null)
            : base(new GALearningStrategy(network, populationFactory), LearningMode.Parallel, test)
        {
            Contract.Requires(network != null);
        }

        public GALearningEpoch(GALearningStrategy strategy, NeuralNetworkTest test = null)
            : base(strategy, LearningMode.Parallel, test)
        {
            Contract.Requires(strategy != null);
        }
        
        #endregion
    }
}
