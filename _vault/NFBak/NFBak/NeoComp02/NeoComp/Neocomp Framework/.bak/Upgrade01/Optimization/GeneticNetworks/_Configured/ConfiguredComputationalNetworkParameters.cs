using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.GeneticNetworks
{
    public abstract class ConfiguredComputationalNetworkParameters : ConfiguredNetworkParameters
    {
        int inputInterfaceLength;

        public int InputInterfaceLength
        {
            get { lock (SyncRoot) return inputInterfaceLength; }
            set
            {
                Contract.Requires(value > 0);

                lock (SyncRoot) inputInterfaceLength = value;
            }
        }

        int outputInterfaceLength;

        public int OutputInterfaceLength
        {
            get { lock (SyncRoot) return outputInterfaceLength; }
            set
            {
                Contract.Requires(value > 0);

                lock (SyncRoot) outputInterfaceLength = value;
            }
        }

        protected override bool IsValid()
        {
            return base.IsValid() &&
                inputInterfaceLength > 0 &&
                outputInterfaceLength > 0;
        }
    }
}
