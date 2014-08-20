using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks.Logical;
using NeoComp.Core;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.GeneticNetworks
{
    public class ConfiguredLogicalNetworkParameters : ConfiguredTestableNetworkParameters
    {
        HashSet<LogicGateType> availableGates;

        public HashSet<LogicGateType> AvailableGates
        {
            get { lock (SyncRoot) return availableGates; }
            set
            {
                Contract.Requires(value != null);

                lock (SyncRoot)
                {
                    availableGates = value;
                    AvailableGateArray = value.ToArray();
                }
            }
        }

        internal LogicGateType[] AvailableGateArray { get; private set; }

        protected override bool IsValid()
        {
            return base.IsValid() && !availableGates.IsNullOrEmpty();
        }
    }
}
