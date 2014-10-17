using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks2.Computational.Neural;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.SupervisedLearning
{
    public abstract class AdjusterLearningEpoch : LearningEpoch
    {
        protected AdjusterLearningEpoch(NeuralNetwork network, NeuralNetworkTest test = null)
            : base(test)
        {
            Contract.Requires(network != null);

            Network = network;
        }
        
        public NeuralNetwork Network { get; private set; }

        internal override void InitializeInternal()
        {
            lock (Network.SyncRoot) base.InitializeInternal();
        }

        internal override void UninitializeInternal()
        {
            lock (Network.SyncRoot) base.UninitializeInternal();
        }
    }
}
