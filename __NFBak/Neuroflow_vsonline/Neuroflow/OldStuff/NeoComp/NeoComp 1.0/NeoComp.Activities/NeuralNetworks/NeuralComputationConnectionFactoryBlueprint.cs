using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.ComputationalNetworks;
using NeoComp;
using NeoComp.NeuralNetworks;

namespace NeoComp.Activities.NeuralNetworks
{
    public abstract class NeuralComputationConnectionFactoryBlueprint<T> : Blueprint<IFactory<ComputationConnection<double>>>
        where T : ComputationConnection<double>
    {
        protected sealed override IFactory<ComputationConnection<double>> CreateObject(System.Activities.NativeActivityContext context)
        {
            return CreateNeuralComputationConnectionFactory(context);
        }

        protected abstract IFactory<T> CreateNeuralComputationConnectionFactory(System.Activities.NativeActivityContext context);
    }
}
