using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Core;

namespace NeoComp.Networks
{
    public static class ComputationalArchitecture
    {
        #region Full Connected

        public static void InitializeFullConnected(
                IComputationalNetwork network,
                int neuronCount,
                Func<Connection> connectionFactoryMethod,
                Func<Node> nodeFactoryMethod,
                bool feedForward = true)
        {
            Contract.Requires(network != null);
            Contract.Requires(connectionFactoryMethod != null);
            Contract.Requires(nodeFactoryMethod != null);
            Contract.Requires(neuronCount > 0);
            Contract.Requires(feedForward == true); // TODO: Non feed-forward

            lock (network.SyncRoot)
            {
                try
                {
                    int inputInterfaceLength = network.InputInterface.Length;
                    int outputInterfaceLength = network.OutputInterface.Length;
                    int neuronBeginIndex = inputInterfaceLength;
                    int neuronEndIndex = neuronBeginIndex + neuronCount - 1;
                    int maxConnectionIndex = inputInterfaceLength + neuronCount + outputInterfaceLength - 1; 
                    
                    for (int idx = neuronBeginIndex; idx <= neuronEndIndex; idx++)
                    {
                        network.AddNode(idx, CreateNode(nodeFactoryMethod));
                    }

                    for (int iidx = 0; iidx < neuronBeginIndex; iidx++)
                    {
                        for (int nidx = neuronBeginIndex; nidx <= neuronEndIndex; nidx++)
                        {
                            network.AddConnection(new ConnectionIndex(iidx, nidx), CreateConnection(connectionFactoryMethod));
                        }
                    }

                    for (int nidx = neuronBeginIndex; nidx <= neuronEndIndex; nidx++)
                    {
                        for (int oidx = nidx + 1; oidx <= maxConnectionIndex; oidx++)
                        {
                            network.AddConnection(new ConnectionIndex(nidx, oidx), CreateConnection(connectionFactoryMethod));
                        }
                    }

                    for (int idx = neuronEndIndex + 1; idx <= maxConnectionIndex; idx++)
                    {
                        network.AddNode(idx, CreateNode(nodeFactoryMethod));
                        network.AddConnection(new ConnectionIndex(idx, idx + inputInterfaceLength), CreateConnection(connectionFactoryMethod));
                    }
                }
                catch (Exception ex)
                {
                    throw GetErrorEx(ex);
                }
            }
        } 

        #endregion

        #region Layered

        public static void InitializeLayered(
                IComputationalNetwork network,
                Func<Connection> connectionFactoryMethod,
                Func<Node> nodeFactoryMethod,
                bool feedForward,
                params int[] neuronCounts)
        {
            Contract.Requires(network != null);
            Contract.Requires(connectionFactoryMethod != null);
            Contract.Requires(nodeFactoryMethod != null);
            Contract.Requires(feedForward == true); // TODO: Non feed-forward
            Contract.Requires(!neuronCounts.IsNullOrEmpty());
            Contract.Requires(Contract.ForAll(neuronCounts, (c) => c > 0));

            lock (network.SyncRoot)
            {
                try
                {
                    int inputInterfaceLength = network.InputInterface.Length;
                    int outputInterfaceLength = network.OutputInterface.Length;
                    int index = inputInterfaceLength;
                    int prevLayerBeginIndex = 0;
                    int prevLayerNeuronCount = inputInterfaceLength;
                    for (int layerIdx = 0; layerIdx < neuronCounts.Length; layerIdx++)
                    {
                        int currentCount = neuronCounts[layerIdx];
                        CreateLayer(network, nodeFactoryMethod, index, currentCount);
                        ConnectLayer(network, connectionFactoryMethod, prevLayerBeginIndex, prevLayerNeuronCount, index, currentCount);
                        prevLayerBeginIndex = index;
                        prevLayerNeuronCount = currentCount;
                        index += currentCount;
                    }

                    CreateLayer(network, nodeFactoryMethod, index, outputInterfaceLength);
                    ConnectLayer(network, connectionFactoryMethod, prevLayerBeginIndex, prevLayerNeuronCount, index, outputInterfaceLength);

                    prevLayerBeginIndex = index;
                    prevLayerNeuronCount = outputInterfaceLength;
                    index += outputInterfaceLength;

                    ConnectLayer(network, connectionFactoryMethod, prevLayerBeginIndex, prevLayerNeuronCount, index, outputInterfaceLength);
                }
                catch (Exception ex)
                {
                    throw GetErrorEx(ex);
                }
            }
        }

        private static void ConnectLayer(
                IComputationalNetwork network, 
                Func<Connection> connectionFactoryMethod, 
                int prevLayerBeginIndex, 
                int prevLayerNeuronCount, 
                int beginIndex, 
                int count)
        {
            for (int pIdx = prevLayerBeginIndex; pIdx < prevLayerBeginIndex + prevLayerNeuronCount; pIdx++)
            {
                for (int idx = beginIndex; idx < beginIndex + count; idx++)
                {
                    network.AddConnection(new ConnectionIndex(pIdx, idx), CreateConnection(connectionFactoryMethod));
                }
            }
        }

        private static void CreateLayer(
                IComputationalNetwork network, 
                Func<Node> nodeFactoryMethod, 
                int beginIndex, 
                int count)
        {
            count = beginIndex + count;
            for (int neuronIndex = beginIndex; neuronIndex < count; neuronIndex++)
            {
                network.AddNode(neuronIndex, CreateNode(nodeFactoryMethod));
            }
        }

        #endregion

        #region Factories

        private static Connection CreateConnection(Func<Connection> connectionFactoryMethod)
        {
            var conn = connectionFactoryMethod();
            if (conn == null) throw new InvalidOperationException("Connection factory method has returned null.");
            return conn;
        }

        private static Node CreateNode(Func<Node> nodeFactoryMethod)
        {
            var node = nodeFactoryMethod();
            if (node == null) throw new InvalidOperationException("Node factory method has returned null.");
            return node;
        }

        #endregion

        #region Ex

        private static InvalidOperationException GetErrorEx(Exception ex)
        {
            return new InvalidOperationException("Network initialization failed. See inner exception for details.", ex);
        }

        #endregion
    }
}
