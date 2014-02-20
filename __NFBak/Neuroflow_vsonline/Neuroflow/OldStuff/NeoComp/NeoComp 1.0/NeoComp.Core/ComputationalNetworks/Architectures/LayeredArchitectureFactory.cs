using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using NeoComp.Networks;

namespace NeoComp.ComputationalNetworks.Architectures
{
    public sealed class LayeredArchitectureFactory<T> : NetworkFactory<NodeLayerDefinition<T>, ConnectionLayerDefinition<T>>
        where T : struct
    {
        internal LayeredArchitectureFactory() { }

        #region Override

        // This is sealed, so it is enought to check factories.

        protected internal override bool OverrideNetworkFactoryEntry(ModifyableNetworkFactoryEntry<NodeLayerDefinition<T>, ConnectionLayerDefinition<T>> factoryEntry, HashSet<int> occupiedNodeIndexes)
        {
            RemoveInvalidConnFacts(factoryEntry.LowerConnectionFactoryEntries);
            RemoveInvalidConnFacts(factoryEntry.UpperConnectionFactoryEntries);
            return
                factoryEntry.NodeFactoryEntry.Index >= 1 && // Node is not on Input Interface pins.
                ((factoryEntry.LowerConnectionFactoryEntries.Count > 0 && factoryEntry.UpperConnectionFactoryEntries.Count > 0) || // Has Output And Input.
                (factoryEntry.NodeFactoryEntry.Index == MaxEntryIndex && factoryEntry.UpperConnectionFactoryEntries.Count > 0)); // On end and has input.
        }

        private void RemoveInvalidConnFacts(IList<ConnectionFactoryEntry<ConnectionLayerDefinition<T>>> collection)
        {
            for (int idx = 0; idx < collection.Count; idx++)
            {
                var entry = collection[idx];
                if (entry.Index.LowerNodeIndex >= 1 && entry.Index.UpperNodeIndex <= MaxEntryIndex - 1)
                {
                    continue;
                }
                else
                {
                    collection.RemoveAt(idx--);
                }
            }
        }

        #endregion
    }
}
