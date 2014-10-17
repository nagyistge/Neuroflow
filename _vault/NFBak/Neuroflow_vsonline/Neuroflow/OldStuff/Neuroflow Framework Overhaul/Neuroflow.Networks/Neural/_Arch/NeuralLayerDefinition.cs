using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core;
using System.Diagnostics.Contracts;

namespace Neuroflow.Networks.Neural
{
    public class NeuralLayerDefinition
    {
        public NeuralLayerDefinition(IFactory<NeuralNode> nodeFactory, int nodeCount, NeuralConnectionDefinition selfConnectionDefinition = null)
        {
            Contract.Requires(nodeFactory != null);
            Contract.Requires(nodeCount > 0);

            NodeFactory = nodeFactory;
            NodeCount = nodeCount;
            SelfConnectionDefinition = selfConnectionDefinition;
        }
        
        public IFactory<NeuralNode> NodeFactory { get; private set; }

        public int NodeCount { get; private set; }
        
        public NeuralConnectionDefinition SelfConnectionDefinition { get; private set; }
    }
}
