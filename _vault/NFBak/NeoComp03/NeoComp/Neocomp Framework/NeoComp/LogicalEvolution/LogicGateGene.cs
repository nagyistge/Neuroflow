using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Networks.Computational.Logical;
using NeoComp.Networks.Computational;

namespace NeoComp.LogicalEvolution
{
    public sealed class LogicGateGene : LogicalNodeGene
    {
        public LogicGateGene(int index, LogicalOperation operation)
            : base(index)
        {
            Contract.Requires(index > 0); 
            
            Operation = operation;
        }

        public LogicalOperation Operation { get; private set; }

        protected internal override ComputationalNode<bool> CreateNode()
        {
            return new LogicGate(Operation);
        }
    }
}
