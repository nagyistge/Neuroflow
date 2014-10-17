using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using NeoComp.Networks;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.GeneticNetworks
{
    public abstract class ConfiguredTestableNetworkParameters : ConfiguredComputationalNetworkParameters
    {
        double functionalErrorTreshold = 0.01;

        public double FunctionalErrorTreshold
        {
            get { lock (SyncRoot) return functionalErrorTreshold; }
            set
            {
                Contract.Requires(value > 0.0);

                lock (SyncRoot) functionalErrorTreshold = value;
            }
        }

        IComputationalNetworkTest test;

        public IComputationalNetworkTest Test
        {
            get { lock (SyncRoot) return test; }
            set
            {
                Contract.Requires(value != null);

                lock (SyncRoot) test = value;
            }
        }

        protected override bool IsValid()
        {
            return base.IsValid() &&
                test != null;
        }
    }
}
