using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks.Neural;
using NeoComp.Core;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.GeneticNetworks
{
    public class ConfiguredNeuralNetworkParameters : ConfiguredTestableNetworkParameters
    {
        IActivationFunction activationFunction;

        public IActivationFunction ActivationFunction
        {
            get { lock (SyncRoot) return activationFunction; }
            set
            {
                Contract.Requires(value != null);
                
                lock (SyncRoot) activationFunction = value;
            }
        }

        protected override bool IsValid()
        {
            return base.IsValid() && activationFunction != null;
        }
    }
}
