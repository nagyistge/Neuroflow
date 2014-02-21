using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Core;
using NeoComp.Adjustables;

namespace NeoComp.Networks
{
    public static class NetworkExtensions
    {
        public static IEnumerable<IAdjustableItem> GetAdjustableItems(this INetwork network)
        {
            Contract.Requires(network != null);

            return network.GetNodes().OfType<IAdjustableItem>().Concat(network.GetConnections().OfType<IAdjustableItem>());
        }
    }
}
