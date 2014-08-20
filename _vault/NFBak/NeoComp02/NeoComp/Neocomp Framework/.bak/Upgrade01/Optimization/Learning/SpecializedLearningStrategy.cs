using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks.Neural;
using System.Diagnostics.Contracts;
using NeoComp.Networks;

namespace NeoComp.Optimization.Learning
{
    public abstract class SpecializedLearningStrategy<TSynapse, TNeuron> : LearningStrategy
        where TSynapse : ISynapse
        where TNeuron : INeuron
    {
        protected SpecializedLearningStrategy(NeuralNetwork network)
            : base(network)
        {
            Contract.Requires(network != null);
        }

        public ConnectedNodeCollection<TSynapse, TNeuron> ConnectedNeurons { get; private set; }

        protected internal override void InitializeNewRun()
        {
            lock (SyncRoot)
            {
                if (ConnectedNeurons == null)
                {
                    lock (Network.SyncRoot)
                    {
                        ConnectedNeurons = Network.ConnectedNodes.CreateView<TSynapse, TNeuron>();
                        if (ConnectedNeurons.Count == 0)
                        {
                            throw new InvalidOperationException(
                                "Specialized neurons of type '" + typeof(TNeuron).FullName + "' not found in network.");
                        }
                    }
                }
            }
        }
    }
}
