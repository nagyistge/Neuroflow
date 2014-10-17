using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks2.Computational;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.SupervisedLearning
{
    public sealed class BackPropagationConnectionContext : BackProgationContext
    {
        internal BackPropagationConnectionContext(object ruleContext, ComputationalConnection<double> connection)
            : base(ruleContext)
        {
            Contract.Requires(connection != null);
            Connection = connection;
        }

        public ComputationalConnection<double> Connection { get; private set; }
    }
}
