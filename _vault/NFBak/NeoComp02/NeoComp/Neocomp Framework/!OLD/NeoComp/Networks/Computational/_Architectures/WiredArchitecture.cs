using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Diagnostics.Contracts;
using System.Diagnostics;

namespace NeoComp.Networks.Computational
{
    [Serializable]
    public abstract class WiredArchitecture<T> : Architecture<T>
    {
        protected WiredArchitecture(
            int inputInterfaceLength, 
            int outputInterfaceLength, 
            int nodeCount,
            IFactory<OperationNode<T>> nodeFactory,
            IFactory<OperationNode<T>> collectorNodeFactory, 
            IFactory<ComputationalConnection<T>> connectionFactory,
            bool recurrent = false)
            : base(inputInterfaceLength, outputInterfaceLength)
        {
            Contract.Requires(inputInterfaceLength > 0);
            Contract.Requires(outputInterfaceLength > 0);
            Contract.Requires(nodeCount > 0);
            Contract.Requires(nodeFactory != null);
            Contract.Requires(collectorNodeFactory != null);
            Contract.Requires(connectionFactory != null);

            NodeCount = nodeCount;
            NodeFactory = nodeFactory;
            CollectorNodeFactory = collectorNodeFactory;
            ConnectionFactory = connectionFactory;
            Recurrent = recurrent;
        }

        public int NodeCount { get; private set; }

        public bool Recurrent { get; private set; }

        public IFactory<OperationNode<T>> NodeFactory { get; private set; }

        public IFactory<OperationNode<T>> CollectorNodeFactory { get; private set; }

        public IFactory<ComputationalConnection<T>> ConnectionFactory { get; private set; }

        protected override void Build(ComputationalNetworkFactory<T> factory)
        {
            int nodeBeginIndex = InputInterfaceLength;
            int nodeEndIndex = nodeBeginIndex + NodeCount - 1;
            int maxConnectionIndex = InputInterfaceLength + NodeCount + OutputInterfaceLength - 1;

            for (int idx = nodeBeginIndex; idx <= nodeEndIndex; idx++)
            {
                factory.AddNodeFactory(idx, NodeFactory);
            }

            for (int iidx = 0; iidx < nodeBeginIndex; iidx++)
            {
                for (int nidx = nodeBeginIndex; nidx <= nodeEndIndex; nidx++)
                {
                    factory.AddConnectionFactory(new ConnectionIndex(iidx, nidx), ConnectionFactory);
                }
            }

            for (int nidx = nodeBeginIndex; nidx <= nodeEndIndex; nidx++)
            {
                for (int oidx = nidx + 1; oidx <= maxConnectionIndex; oidx++)
                {
                    factory.AddConnectionFactory(new ConnectionIndex(nidx, oidx), ConnectionFactory);
                }
            }

            for (int idx = nodeEndIndex + 1; idx <= maxConnectionIndex; idx++)
            {
                factory.AddNodeFactory(idx, CollectorNodeFactory);
                factory.AddConnectionFactory(new ConnectionIndex(idx, idx + OutputInterfaceLength), OutputInterfaceConnectionFactory);
            }

            Debug.Assert(factory.MaxEntryIndex == maxConnectionIndex + OutputInterfaceLength);
        }
    }
}
