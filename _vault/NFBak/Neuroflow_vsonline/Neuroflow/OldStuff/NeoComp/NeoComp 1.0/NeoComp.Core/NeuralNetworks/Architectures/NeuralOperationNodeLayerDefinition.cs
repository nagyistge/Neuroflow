using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.ComputationalNetworks.Architectures;
using System.Diagnostics.Contracts;
using NeoComp.ComputationalNetworks;

namespace NeoComp.NeuralNetworks.Architectures
{
    [Serializable]
    public class NeuralOperationNodeLayerDefinition : OperationNodeLayerDefinition<double>
    {
        public NeuralOperationNodeLayerDefinition(IFactory<OperationNode<double>> nodeFactory, int nodeCount, NeuralConnectionLayerDefinition wireConnectionLayerDefinition = null)
            : base(nodeFactory, nodeCount, wireConnectionLayerDefinition)
        {
            Contract.Requires(nodeFactory != null);
            Contract.Requires(nodeCount > 0);
        }
    }
}
