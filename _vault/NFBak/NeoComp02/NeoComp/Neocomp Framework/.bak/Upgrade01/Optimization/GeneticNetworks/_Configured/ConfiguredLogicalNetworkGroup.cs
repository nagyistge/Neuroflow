using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks.Logical;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.GeneticNetworks
{
    public class ConfiguredLogicalNetworkGroup : ConfiguredNetworkGroup<ConfiguredLogicalNetworkParameters, LogicGateType, LogicalNetwork>
    {
        public ConfiguredLogicalNetworkGroup(ConfiguredLogicalNetworkBodyFactory factory, int size)
            : base(factory, size)
        {
            Contract.Requires(factory != null);
            Contract.Requires(size > 0);
        }
    }
}
