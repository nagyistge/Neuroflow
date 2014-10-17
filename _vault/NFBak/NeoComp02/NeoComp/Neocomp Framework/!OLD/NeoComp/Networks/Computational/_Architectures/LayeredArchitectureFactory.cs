using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.Networks.Computational
{
    public sealed class LayeredArchitectureFactory<T> : NetworkFactory<NodeLayerDefinition<T>, ConnectionLayerDefinition<T>>, INetworkBuildingRules<NodeLayerDefinition<T>, ConnectionLayerDefinition<T>>
    {
        internal LayeredArchitectureFactory() { }

        bool INetworkBuildingRules<NodeLayerDefinition<T>, ConnectionLayerDefinition<T>>.IsValidNodeEntry(NodeEntry<NodeLayerDefinition<T>> entry)
        {
            return true;
        }

        bool INetworkBuildingRules<NodeLayerDefinition<T>, ConnectionLayerDefinition<T>>.IsValidConnectionEntry(NodeEntry<NodeLayerDefinition<T>> parent, ConnectionEntry<ConnectionLayerDefinition<T>> entry)
        {
            return true;
        }

        bool INetworkBuildingRules<NodeLayerDefinition<T>, ConnectionLayerDefinition<T>>.IsValidNetworkEntry(NetworkEntry<NodeLayerDefinition<T>, ConnectionLayerDefinition<T>> entry)
        {
            return (entry.LowerConnectionEntryArray.Length > 0 && entry.UpperConnectionEntryArray.Length > 0) || // Has Output And Input.
                   (entry.NodeEntry.Index == MaxEntryIndex && entry.UpperConnectionEntryArray.Length > 0); // On end and has input.
        }

        bool INetworkBuildingRules<NodeLayerDefinition<T>, ConnectionLayerDefinition<T>>.IsValidNodeFactoryEntry(NodeFactoryEntry<NodeLayerDefinition<T>> entry)
        {
            return entry.Index >= 1; // Node is not on Input Interface pins.
        }

        bool INetworkBuildingRules<NodeLayerDefinition<T>, ConnectionLayerDefinition<T>>.IsValidConnectionFactoryEntry(NodeFactoryEntry<NodeLayerDefinition<T>> parent, ConnectionFactoryEntry<ConnectionLayerDefinition<T>> entry)
        {
            return entry.Index.LowerNodeIndex >= 1 && entry.Index.UpperNodeIndex <= MaxEntryIndex - 1; // I/O area are not recurrent.
        }

        bool INetworkBuildingRules<NodeLayerDefinition<T>, ConnectionLayerDefinition<T>>.IsValidNetworkFactoryEntry(NetworkFactoryEntry<NodeLayerDefinition<T>, ConnectionLayerDefinition<T>> entry)
        {
            return true;
        }
    }
}
