using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using Neuroflow.Core.Networks;

namespace Neuroflow.Core.ComputationalNetworks.Architectures
{
    [Serializable]
    public class StandardMLABuilder<T> : ILayeredArchitectureBuilder<T>
        where T : struct
    {
        public StandardMLABuilder(bool wired, ConnectionLayerDefinition<T> connectionLayerDefinition, int inputInterfaceLength, params NodeLayerDefinition<T>[] nodeLayerDefinitions)
        {
            Contract.Requires(inputInterfaceLength > 0);
            Contract.Requires(connectionLayerDefinition != null);
            Contract.Requires(nodeLayerDefinitions != null);
            Contract.Requires(nodeLayerDefinitions.Length > 0);

            ConnectionLayerDefinition = connectionLayerDefinition;
            this.nodeLayerDefinitions = nodeLayerDefinitions;
            InputInterfaceLength = inputInterfaceLength;
            Wired = wired;
        }
        
        NodeLayerDefinition<T>[] nodeLayerDefinitions;
        
        public ConnectionLayerDefinition<T> ConnectionLayerDefinition { get; private set; }

        public ReadOnlyCollection<NodeLayerDefinition<T>> NodeLayerDefinitions
        {
            get { return new ReadOnlyCollection<NodeLayerDefinition<T>>((IList<NodeLayerDefinition<T>>)nodeLayerDefinitions); }
        }

        public int InputInterfaceLength { get; private set; }

        public int OutputInterfaceLength
        {
            get
            {
                var def = nodeLayerDefinitions[nodeLayerDefinitions.Length - 1];
                var intf = def as IInterfaced;
                return intf == null ? def.NodeCount : intf.InputInterfaceLength;
            }
        }

        public bool Wired { get; private set; }

        void ILayeredArchitectureBuilder<T>.Build(LayeredArchitectureFactory<T> factory)
        {
            for (int uidx = 0; uidx < nodeLayerDefinitions.Length; uidx++)
            {
                int lidx = uidx + 1;
                factory.AddConnectionFactory(new ConnectionIndex(uidx, lidx), ConnectionLayerDefinition.AsFactory());
                factory.AddNodeFactory(lidx, nodeLayerDefinitions[uidx].AsFactory());
            }
        }
    }
}
