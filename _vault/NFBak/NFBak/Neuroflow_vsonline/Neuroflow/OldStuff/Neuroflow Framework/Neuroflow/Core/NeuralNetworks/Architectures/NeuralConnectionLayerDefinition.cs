using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core.ComputationalNetworks.Architectures;
using System.Diagnostics.Contracts;
using Neuroflow.Core.ComputationalNetworks;

namespace Neuroflow.Core.NeuralNetworks.Architectures
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
