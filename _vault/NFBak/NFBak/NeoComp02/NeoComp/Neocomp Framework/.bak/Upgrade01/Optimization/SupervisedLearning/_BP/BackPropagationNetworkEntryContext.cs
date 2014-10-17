using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.SupervisedLearning
{
    public sealed class BackPropagationNetworkEntryContext
    {
        internal BackPropagationNetworkEntryContext(BackPropgationNodeContext nodeContext, BackPropagationConnectionContext[] upperConnectionContexts, BackPropagationConnectionContext[] lowerConnectionContexts)
        {
            Contract.Requires(nodeContext != null);
            Contract.Requires(upperConnectionContexts != null);
            Contract.Requires(lowerConnectionContexts != null);

            this.nodeContext = nodeContext;
            this.upperConnectionContexts = upperConnectionContexts;
            this.lowerConnectionContexts = lowerConnectionContexts;
        }

        BackPropgationNodeContext nodeContext;

        public BackPropgationNodeContext NodeContext
        {
            get { return nodeContext; }
        }

        BackPropagationConnectionContext[] upperConnectionContexts;

        public BackPropagationConnectionContext[] UpperConnectionContexts
        {
            get { return upperConnectionContexts; }
        }

        BackPropagationConnectionContext[] lowerConnectionContexts;

        public BackPropagationConnectionContext[] LowerConnectionContexts
        {
            get { return lowerConnectionContexts; }
        }
    }
}
