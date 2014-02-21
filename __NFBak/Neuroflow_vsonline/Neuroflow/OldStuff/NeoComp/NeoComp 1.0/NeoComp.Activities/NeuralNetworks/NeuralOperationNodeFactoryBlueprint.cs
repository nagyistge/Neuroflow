using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.ComputationalNetworks;
using NeoComp;

namespace NeoComp.Activities.NeuralNetworks
{
    public abstract class NeuralOperationNodeFactoryBlueprint<T> : Blueprint<IFactory<OperationNode<double>>>
        where T : OperationNode<double>
    {
        protected override IFactory<OperationNode<double>> CreateObject(System.Activities.NativeActivityContext context)
        {
            return CreateNeuralOperationNodeFactory(context);
        }

        protected abstract IFactory<T> CreateNeuralOperationNodeFactory(System.Activities.NativeActivityContext context);
    }
}
