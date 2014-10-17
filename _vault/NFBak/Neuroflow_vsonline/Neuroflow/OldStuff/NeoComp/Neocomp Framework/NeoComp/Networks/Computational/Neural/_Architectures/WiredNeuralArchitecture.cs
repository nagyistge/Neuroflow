using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Core;

namespace NeoComp.Networks.Computational.Neural
{
    public sealed class WiredNeuralArchitecture : WiredArchitecture<double>, INeuralArchitecture
    {
        public WiredNeuralArchitecture(
            int inputInterfaceLength, 
            int outputInterfaceLength, 
            int nodeCount,
            IFactory<OperationNode<double>> nodeFactory,
            IFactory<OperationNode<double>> collectorNodeFactory, 
            IFactory<ComputationalConnection<double>> connectionFactory,
            bool recurrent = false)
            : base(inputInterfaceLength, outputInterfaceLength, nodeCount, nodeFactory, collectorNodeFactory, connectionFactory, recurrent)
        {
            Contract.Requires(inputInterfaceLength > 0);
            Contract.Requires(outputInterfaceLength > 0);
            Contract.Requires(nodeCount > 0);
            Contract.Requires(nodeFactory != null);
            Contract.Requires(collectorNodeFactory != null);
            Contract.Requires(connectionFactory != null);
        }

        protected sealed override IFactory<ComputationalConnection<double>> OutputInterfaceConnectionFactory
        {
            get { return new Factory<NeuralConnection>(); }
        }

        protected sealed override ComputationalNetworkFactory<double> CreateFactoryInstance()
        {
            return new NeuralNetworkFactory(InputInterfaceLength, OutputInterfaceLength);
        }

        protected sealed override ComputationalNetwork<double> CreateNetworkInstance(ComputationalNetworkFactory<double> factory)
        {
            return new NeuralNetwork((NeuralNetworkFactory)factory);
        }

        new public NeuralNetworkFactory CreateFactory()
        {
            return (NeuralNetworkFactory)base.CreateFactory();
        }

        new public NeuralNetwork CreateNetwork()
        {
            return (NeuralNetwork)base.CreateNetwork();
        }
    }
}
