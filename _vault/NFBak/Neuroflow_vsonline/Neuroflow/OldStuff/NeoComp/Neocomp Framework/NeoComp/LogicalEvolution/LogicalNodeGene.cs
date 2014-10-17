using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Networks.Computational;

namespace NeoComp.LogicalEvolution
{
    [ContractClass(typeof(LogicalNodeGeneContract))]
    public abstract class LogicalNodeGene : LogicalNetworkGene
    {
        protected LogicalNodeGene(int index)
            : base(index)
        {
            Contract.Requires(index > 0);
        }
        
        protected internal abstract ComputationalNode<bool> CreateNode();
    }

    [ContractClassFor(typeof(LogicalNodeGene))]
    abstract class LogicalNodeGeneContract : LogicalNodeGene
    {
        protected LogicalNodeGeneContract() : base(0)
        {
        }
        
        protected internal override ComputationalNode<bool> CreateNode()
        {
            Contract.Ensures(Contract.Result<ComputationalNode<bool>>() != null);
            return null;
        }
    }
}
