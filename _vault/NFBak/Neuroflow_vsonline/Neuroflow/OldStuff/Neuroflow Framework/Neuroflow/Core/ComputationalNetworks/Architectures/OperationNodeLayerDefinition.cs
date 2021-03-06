﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core;
using System.Diagnostics.Contracts;

namespace Neuroflow.Core.ComputationalNetworks.Architectures
{
    [Serializable]
    public class OperationNodeLayerDefinition<T> : NodeLayerDefinition<T>
        where T : struct
    {
        public OperationNodeLayerDefinition(IFactory<OperationNode<T>> nodeFactory, int nodeCount, ConnectionLayerDefinition<T> wireConnectionLayerDefinition = null)
            : base(nodeFactory, nodeCount)
        {
            Contract.Requires(nodeFactory != null);
            Contract.Requires(nodeCount > 0);

            WireConnectionLayerDefinition = wireConnectionLayerDefinition;
        }

        new public IFactory<OperationNode<T>> NodeFactory
        {
            get { return (IFactory<OperationNode<T>>)base.NodeFactory; }
        }

        public ConnectionLayerDefinition<T> WireConnectionLayerDefinition { get; private set; }
    }
}
