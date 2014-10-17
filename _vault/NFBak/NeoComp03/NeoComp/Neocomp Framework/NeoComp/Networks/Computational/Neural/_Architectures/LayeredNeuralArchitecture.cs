using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Core;

namespace NeoComp.Networks.Computational.Neural
{
    [Serializable]
    public class LayeredNeuralArchitecture : LayeredArchitecture<double>, INeuralArchitecture
    {
        public LayeredNeuralArchitecture(ILayeredArchitectureBuilder<double> builder)
            : base(builder)
        {
            Contract.Requires(builder != null);
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
