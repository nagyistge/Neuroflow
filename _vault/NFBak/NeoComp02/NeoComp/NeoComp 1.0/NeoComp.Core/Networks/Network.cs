using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using NeoComp;
using NeoComp.Internal;
using System.Runtime.Serialization;

namespace NeoComp.Networks
{
    [Serializable]
    [DataContract(Name = "network", IsReference = true, Namespace = xmlns.NeoCompNS)]
    public abstract class Network<TNode, TConnection> : ICloneable
    {
        #region Constructor

        protected Network(NetworkFactory<TNode, TConnection> factory)
        {
            Contract.Requires(factory != null);

            Build(factory);
        } 

        #endregion

        #region Properties

        [DataMember(Name = "entries")]
        internal NetworkEntry<TNode, TConnection>[] EntryArray { get; private set; }

        public ReadOnlyCollection<NetworkEntry<TNode, TConnection>> Entries
        {
            get { return new ReadOnlyCollection<NetworkEntry<TNode, TConnection>>((IList<NetworkEntry<TNode, TConnection>>)EntryArray); }
        }

        [DataMember(Name = "isFeedForward")]
        public bool IsFeedForward { get; private set; }

        [DataMember(Name = "maxEntryIndex")]
        protected internal int MaxEntryIndex { get; private set; }

        [DataMember(Name = "nNodes")]
        public int NumberOfNodes { get; private set; }

        [DataMember(Name = "nConns")]
        public int NumberOfConnections { get; private set; }

        #endregion

        #region Build

        private void Build(NetworkFactory<TNode, TConnection> factory)
        {
            lock (factory.SyncRoot)
            {
                var entries = new LinkedList<NetworkEntry<TNode, TConnection>>();
                var nodeIndexes = new HashSet<int>();
                var connIndexes = new HashSet<ConnectionIndex>();
                using (var access = factory.GetEntryCollectionAccess())
                {
                    MaxEntryIndex = factory.MaxEntryIndex;
                    IsFeedForward = true;

                    foreach (var factoryEntry in access)
                    {
                        if (factoryEntry.NodeFactoryEntry != null)
                        {
                            // Has node.

                            var modFactEntry = new ModifyableNetworkFactoryEntry<TNode, TConnection>(factoryEntry);
                            if (factory.OverrideNetworkFactoryEntry(modFactEntry, nodeIndexes))
                            {
                                var modNetEntry = modFactEntry.CreateModifyableNetworkEntry();
                                if (factory.OverrideNetworkEntry(modNetEntry, nodeIndexes))
                                {
                                    var netEntry = modNetEntry.CreateNetworkEntry();
                                    entries.AddLast(netEntry);
                                    nodeIndexes.Add(netEntry.NodeEntry.Index);
                                    foreach (var uce in netEntry.UpperConnectionEntryArray) connIndexes.Add(uce.Index);
                                    foreach (var lce in netEntry.LowerConnectionEntryArray) connIndexes.Add(lce.Index);

                                    // Ok. Determine if it is non FF:
                                    foreach (var lc in netEntry.LowerConnectionEntryArray)
                                    {
                                        Contract.Assert(netEntry.NodeEntry.Index == lc.Index.UpperNodeIndex);

                                        if (lc.Index.LowerNodeIndex <= lc.Index.UpperNodeIndex) IsFeedForward = false;
                                    }
                                }
                            }
                        }
                    }
                    
                    EntryArray = entries.ToArray();
                    NumberOfNodes = nodeIndexes.Count;
                    NumberOfConnections = connIndexes.Count;
                }
            }
        }

        #endregion

        #region Item Access

        public virtual NetworkItems<TNode, TConnection> GetItems()
        {
            var nodes = new Dictionary<int, NodeEntry<TNode>>();
            var conns = new Dictionary<ConnectionIndex, ConnectionEntry<TConnection>>();
            foreach (var ne in EntryArray)
            {
                foreach (var uc in ne.UpperConnectionEntryArray) conns[uc.Index] = uc;
                nodes[ne.NodeEntry.Index] = ne.NodeEntry;
                foreach (var lc in ne.LowerConnectionEntryArray) conns[lc.Index] = lc;
            }
            return new NetworkItems<TNode, TConnection>(nodes.Values, conns.Values);
        }

        #endregion

        #region Clone

        public Network<TNode, TConnection> Clone()
        {
            return CloneHelper.Clone(this);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        #endregion
    }
}
