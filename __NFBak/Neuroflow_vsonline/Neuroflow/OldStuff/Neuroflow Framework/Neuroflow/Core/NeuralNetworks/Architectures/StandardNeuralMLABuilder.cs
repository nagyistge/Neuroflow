using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Neuroflow.Core.ComputationalNetworks;
using Neuroflow.Core.ComputationalNetworks.Architectures;

namespace Neuroflow.Core.NeuralNetworks.Architectures
{
    [Serializable]
    public class StandardNeuralMLABuilder : StandardMLABuilder<double>
    {
        public StandardNeuralMLABuilder(bool wired, ConnectionLayerDefinition<double> connectionLayerDefinition, int inputInterfaceLength, params NodeLayerDefinition<double>[] nodeLayerDefinitions)
            : base(wired, connectionLayerDefinition, inputInterfaceLength, nodeLayerDefinitions)
        {
            Contract.Requires(connectionLayerDefinition != null);
            Contract.Requires(nodeLayerDefinitions != null);
            Contract.Requires(nodeLayerDefinitions.Length > 0);
            Contract.Requires(inputInterfaceLength > 0);
        }
    }
}
