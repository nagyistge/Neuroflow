using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using NeoComp.Networks.Computational.Logical;
using NeoComp.Networks;
using NeoComp.Networks.Computational;

namespace NeoComp.LogicalEvolution
{
    internal static class LogicalNetworkDNADecoder
    {
        internal static LogicalNetwork CreateNetwork(LogicalNetworkFactory factory, IEnumerable<LogicalNetworkGene> geneSequence)
        {
            int nodeIndex = 0;
            int lastNodeIndex = -1;
            var uppers = new List<LogicalConnectionGene>();
            var lowers = new List<LogicalConnectionGene>();

            foreach (var gene in geneSequence)
            {
                var connGene = gene as LogicalConnectionGene;
                if (connGene != null)
                {
                    if (connGene.IsUpper) uppers.Add(connGene); else lowers.Add(connGene);
                }
                else
                {
                    var nodeGene = (LogicalNodeGene)gene;
                    nodeIndex = nodeGene.Index;
                    foreach (var ucg in uppers)
                    {
                        int uni = nodeIndex + ucg.Index;
                        if (uni >= 0)
                        {
                            var connIndex = new ConnectionIndex(uni, nodeIndex);
                            factory.TryAddConnectionFactory(connIndex, new Factory<ComputationalConnection<bool>>(ucg.CreateConnection));
                        }
                    }
                    if (lastNodeIndex != -1)
                    {
                        foreach (var lcg in lowers)
                        {
                            int lni = lastNodeIndex + lcg.Index;
                            if (lni >= 0)
                            {
                                var connIndex = new ConnectionIndex(lastNodeIndex, lni);
                                factory.TryAddConnectionFactory(connIndex, new Factory<ComputationalConnection<bool>>(lcg.CreateConnection));
                            }
                        }
                    }
                    factory.TryAddNodeFactory(nodeIndex, new Factory<ComputationalNode<bool>>(nodeGene.CreateNode));
                    lastNodeIndex = nodeIndex;
                    uppers.Clear();
                    lowers.Clear();
                }
            }

            var network = new LogicalNetwork(factory);
            return network;
        }
    }
}
