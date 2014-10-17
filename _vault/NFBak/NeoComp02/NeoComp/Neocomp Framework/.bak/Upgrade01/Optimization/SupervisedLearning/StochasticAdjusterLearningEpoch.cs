using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Networks2.Computational.Neural;
using NeoComp.Adjustables;

namespace NeoComp.Optimization.SupervisedLearning
{
    public abstract class StochasticAdjusterLearningEpoch : AdjusterLearningEpoch
    {
        protected StochasticAdjusterLearningEpoch(NeuralNetwork network, NeuralNetworkTest test = null)
            : base(network, test)
        {
            Contract.Requires(network != null);
        }

        protected sealed override void InitializeNewRun()
        {
            Initialize(Network.GetAdjustableItems());
        }

        protected abstract void Initialize(IEnumerable<IAdjustableItem> items);
    }
}
