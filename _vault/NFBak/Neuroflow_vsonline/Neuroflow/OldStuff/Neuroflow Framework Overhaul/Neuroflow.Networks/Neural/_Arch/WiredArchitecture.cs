using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Neuroflow.Core;
using System.Diagnostics;
using System.ComponentModel.DataAnnotations;
using Neuroflow.Core.ComponentModel;
using System.ComponentModel;

namespace Neuroflow.Networks.Neural
{
    public sealed class WiredArchitecture : Architecture
    {
        public WiredArchitecture(
            [Required]
            [FreeDisplayName("Input Interface Length")]
            [Category(PropertyCategories.Facade)]
            int inputInterfaceLength,
            [Required]
            [FreeDisplayName("Output Interface Length")]
            [Category(PropertyCategories.Facade)]
            int outputInterfaceLength,
            [Required]
            [FreeDisplayName("Node Count")]
            [Category(PropertyCategories.Structure)]
            int nodeCount,
            [Required]
            [FreeDisplayName("Node Factory")]
            [Category(PropertyCategories.Structure)]
            IFactory<NeuralNode> nodeFactory,
            [Required]
            [FreeDisplayName("Collector Node Factory")]
            [Category(PropertyCategories.Structure)]
            IFactory<NeuralNode> collectorNodeFactory,
            [Required]
            [FreeDisplayName("Connection Factory")]
            [Category(PropertyCategories.Structure)]
            IFactory<NeuralConnection> connectionFactory,
            [FreeDisplayName("Is Recurrent")]
            [Category(PropertyCategories.Structure)]
            [InitValue(false)]
            bool recurrent = false)
            : base(inputInterfaceLength, outputInterfaceLength)
        {
            Contract.Requires(inputInterfaceLength >= 0);
            Contract.Requires(outputInterfaceLength >= 0);

            NodeCount = nodeCount;
            NodeFactory = nodeFactory;
            CollectorNodeFactory = collectorNodeFactory;
            ConnectionFactory = connectionFactory;
            Recurrent = recurrent;
        }

        public int NodeCount { get; private set; }

        public bool Recurrent { get; private set; }

        public IFactory<NeuralNode> NodeFactory { get; private set; }

        public IFactory<NeuralNode> CollectorNodeFactory { get; private set; }

        public IFactory<NeuralConnection> ConnectionFactory { get; private set; }

        protected override NetworkDefinition<NeuralNode, NeuralConnection> CreateDefinition()
        {
            var networkDef = new NetworkDefinition<NeuralNode, NeuralConnection>();

            int nodeBeginIndex = InputInterfaceLength;
            int nodeEndIndex = nodeBeginIndex + NodeCount - 1;
            int maxConnectionIndex = InputInterfaceLength + NodeCount + OutputInterfaceLength - 1;

            // Nodes:
            for (int idx = nodeBeginIndex; idx <= nodeEndIndex; idx++)
            {
                networkDef.AddNode(idx, NodeFactory.Create());
            }

            // Input:
            for (int iidx = 0; iidx < nodeBeginIndex; iidx++)
            {
                for (int nidx = nodeBeginIndex; nidx <= nodeEndIndex; nidx++)
                {
                    networkDef.AddConnection(new ConnectionIndex(iidx, nidx), ConnectionFactory.Create());
                }
            }

            // Hidden:
            for (int nidx = nodeBeginIndex; nidx <= nodeEndIndex; nidx++)
            {
                for (int oidx = nidx + 1; oidx <= maxConnectionIndex; oidx++)
                {
                    networkDef.AddConnection(new ConnectionIndex(nidx, oidx), ConnectionFactory.Create());
                    if (Recurrent) networkDef.AddConnection(new ConnectionIndex(oidx, nidx), ConnectionFactory.Create());
                }
            }

            // Output:
            for (int idx = nodeEndIndex + 1; idx <= maxConnectionIndex; idx++)
            {
                networkDef.AddNode(idx, CollectorNodeFactory.Create());
                networkDef.AddConnection(new ConnectionIndex(idx, idx + OutputInterfaceLength), new NeuralConnection());
            }

            Debug.Assert(networkDef.MaxNodeIndex == maxConnectionIndex + OutputInterfaceLength);

            return networkDef;
        }
    }
}
