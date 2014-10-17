using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Neuroflow.Core;
using Neuroflow.Core.ComputationalNetworks;
using Neuroflow.Core.ComputationalNetworks.Architectures;
using System.ComponentModel.DataAnnotations;
using Neuroflow.ComponentModel;
using System.ComponentModel;

namespace Neuroflow.Core.NeuralNetworks.Architectures
{
    public sealed class WiredNeuralArchitecture : WiredArchitecture<double>, INeuralArchitecture
    {
        public WiredNeuralArchitecture(
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
            IFactory<OperationNode<double>> nodeFactory,
            [Required]
            [FreeDisplayName("Collector Node Factory")]
            [Category(PropertyCategories.Structure)]
            IFactory<OperationNode<double>> collectorNodeFactory,
            [Required]
            [FreeDisplayName("Connection Factory")]
            [Category(PropertyCategories.Structure)]
            IFactory<ComputationConnection<double>> connectionFactory,
            [FreeDisplayName("Is Recurrent")]
            [Category(PropertyCategories.Structure)]
            [InitValue(false)]
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

        protected sealed override IFactory<ComputationConnection<double>> OutputInterfaceConnectionFactory
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
