using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks2.Computational;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.SupervisedLearning
{
    public sealed class BackPropgationNodeContext : BackProgationContext
    {
        internal BackPropgationNodeContext(object ruleContext, ComputationalNode<double> node)
            : base(ruleContext)
        {
            Contract.Requires(node != null);
            Node = node;
        }

        public ComputationalNode<double> Node { get; private set; }
    }
}
