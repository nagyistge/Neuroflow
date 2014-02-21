using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Diagnostics.Contracts;

namespace NeoComp.Networks.Computational
{
    [Serializable]
    public abstract class NodeLayerDefinition<T>
    {
        protected NodeLayerDefinition(IFactory<ComputationalNode<T>> nodeFactory, int nodeCount)
        {
            Contract.Requires(nodeFactory != null);
            Contract.Requires(nodeCount > 0);

            NodeFactory = nodeFactory;
            NodeCount = nodeCount;
        }

        public IFactory<ComputationalNode<T>> NodeFactory { get; private set; }

        public int NodeCount { get; private set; }
    }
}
