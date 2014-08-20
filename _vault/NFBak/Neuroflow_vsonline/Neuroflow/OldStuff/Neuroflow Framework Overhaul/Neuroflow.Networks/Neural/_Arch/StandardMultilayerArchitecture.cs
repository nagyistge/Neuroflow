using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Neuroflow.Networks.Neural
{
    public sealed class StandardMultilayerArchitecture : MultilayerArchitecture
    {
        public StandardMultilayerArchitecture(
            NeuralConnectionDefinition connectionDefinition, 
            int inputInterfaceLength, 
            params NeuralLayerDefinition[] layerDefinitions)
            : base(inputInterfaceLength, layerDefinitions[layerDefinitions.Length - 1].NodeCount)
        {
            Contract.Requires(connectionDefinition != null);
            Contract.Requires(layerDefinitions != null);
            Contract.Requires(layerDefinitions.Length > 0);
            Contract.Requires(inputInterfaceLength > 0);

            for (int uidx = 0; uidx < layerDefinitions.Length; uidx++)
            {
                int lidx = uidx + 1;
                Definition.AddConnection(new ConnectionIndex(uidx, lidx), connectionDefinition);
                Definition.AddNode(lidx, layerDefinitions[uidx]);
            }
        }
    }
}
