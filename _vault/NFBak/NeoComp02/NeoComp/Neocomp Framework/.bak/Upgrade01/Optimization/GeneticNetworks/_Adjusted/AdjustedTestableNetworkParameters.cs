using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks;
using NeoComp.Core;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.GeneticNetworks
{
    public class AdjustedTestableNetworkParameters : AdjustedNetworkParameters
    {
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
