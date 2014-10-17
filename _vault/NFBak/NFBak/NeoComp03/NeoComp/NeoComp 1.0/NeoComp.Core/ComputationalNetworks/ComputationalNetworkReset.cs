using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Computations;
using System.Diagnostics.Contracts;

namespace NeoComp.ComputationalNetworks
{
    internal sealed class ComputationalNetworkReset<T> : IReset
        where T : struct
    {
        internal ComputationalNetworkReset(ComputationalNetwork<T> network)
        {
            Contract.Requires(network != null);

            this.network = network;
            var items = network.GetItems();
            nodeResetHandlers = items.NodeEntries.Select(e => new ResetHandler(e.Node)).Where(h => h.IsResetable).ToArray();
            connResetHandlers = items.ConnectionEntries.Select(e => new ResetHandler(e.Connection)).Where(h => h.IsResetable).ToArray();
        }
        
        ComputationalNetwork<T> network;

        ResetHandler[] nodeResetHandlers;

        ResetHandler[] connResetHandlers;

        public void Reset()
        {
            foreach (var h in connResetHandlers) h.Reset();
            foreach (var h in nodeResetHandlers) h.Reset();
            network.Reset();
        }
    }
}
