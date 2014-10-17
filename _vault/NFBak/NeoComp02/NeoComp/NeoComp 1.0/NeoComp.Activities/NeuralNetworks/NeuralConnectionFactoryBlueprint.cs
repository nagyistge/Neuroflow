using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.NeuralNetworks;
using NeoComp;
using System.Activities;

namespace NeoComp.Activities.NeuralNetworks
{
    public sealed class NeuralConnectionFactoryBlueprint : NeuralComputationConnectionFactoryBlueprint<NeuralConnection>
    {
        protected override IFactory<NeuralConnection> CreateNeuralComputationConnectionFactory(System.Activities.NativeActivityContext context)
        {
            return new Factory<NeuralConnection>();
        }

        protected override Activity CreateActivityTemplate(System.Windows.DependencyObject target)
        {
            return new NeuralConnectionFactoryBlueprint { DisplayName = "Neural Connection" };
        }
    }
}
