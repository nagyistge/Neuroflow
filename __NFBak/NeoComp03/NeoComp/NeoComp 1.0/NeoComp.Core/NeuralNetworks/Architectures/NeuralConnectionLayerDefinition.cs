using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.ComputationalNetworks.Architectures;
using System.Diagnostics.Contracts;
using NeoComp.ComputationalNetworks;

namespace NeoComp.NeuralNetworks.Architectures
{
    [Serializable]
    public class NeuralConnectionLayerDefinition : ConnectionLayerDefinition<double>
    {
        public NeuralConnectionLayerDefinition(IFactory<ComputationConnection<double>> connectionFactory, bool recurrent = false)
            : base(connectionFactory, recurrent)
        {
            Contract.Requires(connectionFactory != null);
        }
    }
}
