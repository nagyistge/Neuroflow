using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Collections.ObjectModel;
using NeoComp.Networks;

namespace NeoComp.ComputationalNetworks
{
    public abstract class ComputationalNetworkFactory<T> : 
        NetworkFactory<ComputationNode<T>, ComputationConnection<T>>,
        IInterfaced
        where T : struct
    {
        #region Constructors

        protected ComputationalNetworkFactory(int inputInterfaceLength, int outputInterfaceLength)
        {
            Contract.Requires(inputInterfaceLength > 0);
            Contract.Requires(outputInterfaceLength > 0);

            InputInterfaceLength = inputInterfaceLength;
            OutputInterfaceLength = outputInterfaceLength;
        }

        protected ComputationalNetworkFactory(ComputationalNetwork<T> network)
            : base(network)
        {
            Contract.Requires(network != null);

            InputInterfaceLength = network.InputInterface.Length;
            OutputInterfaceLength = network.OutputInterface.Length;
        } 

        #endregion

        #region Properties

        public int InputInterfaceLength { get; private set; }

        public int OutputInterfaceLength { get; private set; } 

        #endregion

        #region Overrides

        protected internal override bool OverrideNetworkFactoryEntry(ModifyableNetworkFactoryEntry<ComputationNode<T>, ComputationConnection<T>> factoryEntry, HashSet<int> occupiedNodeIndexes)
        {
            RemoveIORecurrentConnFactories(factoryEntry.UpperConnectionFactoryEntries);
            RemoveIORecurrentConnFactories(factoryEntry.LowerConnectionFactoryEntries);

            for (int upperIdx = 0; upperIdx < factoryEntry.UpperConnectionFactoryEntries.Count; upperIdx++)
            {
                var entry = factoryEntry.UpperConnectionFactoryEntries[upperIdx];
                int connectedToIndex = entry.Index.UpperNodeIndex;
                if (entry.Index.UpperNodeIndex < entry.Index.LowerNodeIndex &&
                    !(occupiedNodeIndexes.Contains(connectedToIndex) || connectedToIndex < InputInterfaceLength))
                {
                    factoryEntry.UpperConnectionFactoryEntries.RemoveAt(upperIdx--);
                }
            }

            for (int lowerIdx = 0; lowerIdx < factoryEntry.LowerConnectionFactoryEntries.Count; lowerIdx++)
            {
                var entry = factoryEntry.LowerConnectionFactoryEntries[lowerIdx];
                int connectedToIndex = entry.Index.LowerNodeIndex;
                if (entry.Index.LowerNodeIndex < entry.Index.UpperNodeIndex && !occupiedNodeIndexes.Contains(entry.Index.LowerNodeIndex))
                {
                    factoryEntry.LowerConnectionFactoryEntries.RemoveAt(lowerIdx--);
                }
            }

            return
                factoryEntry.LowerConnectionFactoryEntries.Count != 0 &&
                factoryEntry.UpperConnectionFactoryEntries.Count != 0 &&
                IsNotOnIOPins(factoryEntry.NodeFactoryEntry.Index);
        }

        protected internal override bool OverrideNetworkEntry(ModifyableNetworkEntry<ComputationNode<T>, ComputationConnection<T>> networkEntry, HashSet<int> occupiedNodeIndexes)
        {
            RemoveIORecurrentConns(networkEntry.UpperConnectionEntries);
            RemoveIORecurrentConns(networkEntry.LowerConnectionEntries);
            return
                networkEntry.LowerConnectionEntries.Count != 0 &&
                networkEntry.UpperConnectionEntries.Count != 0 &&
                IsNotOnIOPins(networkEntry.NodeEntry.Index);
        }

        private void RemoveIORecurrentConnFactories(IList<ConnectionFactoryEntry<ComputationConnection<T>>> entryFactColl)
        {
            for (int idx = 0; idx < entryFactColl.Count; idx++)
            {
                var entry = entryFactColl[idx];
                if (IsNotIORecurrent(entry.Index))
                {
                    // I/O area are not recurrent.
                    continue;
                }
                else
                {
                    entryFactColl.RemoveAt(idx--);
                }
            }
        }

        private void RemoveIORecurrentConns(IList<ConnectionEntry<ComputationConnection<T>>> entryFactColl)
        {
            for (int idx = 0; idx < entryFactColl.Count; idx++)
            {
                var entry = entryFactColl[idx];
                if (IsNotIORecurrent(entry.Index))
                {
                    // I/O area are not recurrent.
                    continue;
                }
                else
                {
                    entryFactColl.RemoveAt(idx--);
                }
            }
        }

        private bool IsNotIORecurrent(ConnectionIndex index)
        {
            return index.LowerNodeIndex >= InputInterfaceLength && index.UpperNodeIndex <= MaxEntryIndex - OutputInterfaceLength;
        }

        private bool IsNotOnIOPins(int nodeIndex)
        {
            return nodeIndex >= InputInterfaceLength && nodeIndex <= MaxEntryIndex - OutputInterfaceLength;
        }

        #endregion
    }
}
