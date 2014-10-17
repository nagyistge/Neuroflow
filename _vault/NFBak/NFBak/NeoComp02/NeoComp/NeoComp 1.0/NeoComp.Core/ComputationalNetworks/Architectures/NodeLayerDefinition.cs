using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp;
using System.Diagnostics.Contracts;

namespace NeoComp.ComputationalNetworks.Architectures
{
    [Serializable]
    public abstract class NodeLayerDefinition<T>
        where T : struct
    {
        protected NodeLayerDefinition(IFactory<ComputationNode<T>> nodeFactory, int nodeCount)
        {
            Contract.Requires(nodeFactory != null);
            Contract.Requires(nodeCount > 0);

            NodeFactory = nodeFactory;
            NodeCount = nodeCount;
        }

        public IFactory<ComputationNode<T>> NodeFactory { get; private set; }

        public int NodeCount { get; private set; }
    }
}
