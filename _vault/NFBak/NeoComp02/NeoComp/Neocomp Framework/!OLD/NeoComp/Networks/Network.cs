using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using NeoComp.Core;
using System.Runtime.Serialization;

namespace NeoComp.Networks
{
    [Serializable]
    [DataContract(Name = "network", IsReference = true, Namespace = NeoComp.xmlns)]
    public abstract class Network<TNode, TConnection> : ICloneable
    {
        #region Rule Wrapper Class

        class RuleWrapper : INetworkBuildingRules<TNode, TConnection>
        {
            internal RuleWrapper(INetworkBuildingRules<TNode, TConnection> rules)
            {
                this.rules = rules;
            }

            INetworkBuildingRules<TNode, TConnection> rules;

            bool INetworkBuildingRules<TNode, TConnection>.IsValidNodeEntry(NodeEntry<TNode> entry)
            {
                return rules == null ? true : rules.IsValidNodeEntry(entry);
            }

            bool INetworkBuildingRules<TNode, TConnection>.IsValidConnectionEntry(NodeEntry<TNode> parent, ConnectionEntry<TConnection> entry)
            {
                return rules == null ? true : rules.IsValidConnectionEntry(parent, entry);
            }

            bool INetworkBuildingRules<TNode, TConnection>.IsValidNetworkEntry(NetworkEntry<TNode, TConnection> entry)
            {
                return rules == null ? true : rules.IsValidNetworkEntry(entry);
            }

            bool INetworkBuildingRules<TNode, TConnection>.IsValidNodeFactoryEntry(NodeFactoryEntry<TNode> entry)
            {
                return rules == null ? true : rules.IsValidNodeFactoryEntry(entry);
            }

            bool INetworkBuildingRules<TNode, TConnection>.IsValidConnectionFactoryEntry(NodeFactoryEntry<TNode> parent, ConnectionFactoryEntry<TConnection> entry)
            {
                return rules == null ? true : rules.IsValidConnectionFactoryEntry(parent, entry);
            }

            bool INetworkBuildingRules<TNode, TConnection>.IsValidNetworkFactoryEntry(NetworkFactoryEntry<TNode, TConnection> entry)
            {
                return rules == null ? true : rules.IsValidNetworkFactoryEntry(entry);
            }
        }

        #endregion

        #region Constructor

        protected Network(NetworkFactory<TNode, TConnection> factory, INetworkBuildingRules<TNode, TConnection> rules = null)
        {
            Contract.Requires(factory != null);

            Build(factory, new RuleWrapper(rules));
        } 

        #endregion

        #region Properties

        [DataMember(Name = "entries")]
        internal NetworkEntry<TNode, TConnection>[] EntryArray { get; private set; }

        public ReadOnlyCollection<NetworkEntry<TNode, TConnection>> Entries
        {
            get { return new ReadOnlyCollection<NetworkEntry<TNode, TConnection>>((IList<NetworkEntry<TNode, TConnection>>)EntryArray); }
        }

        [DataMember(Name = "maxEntryIndex")]
        protected internal int MaxEntryIndex { get; private set; } 

        #endregion

        #region Build

        private void Build(NetworkFactory<TNode, TConnection> factory, INetworkBuildingRules<TNode, TConnection> rules)
        {
            lock (factory.SyncRoot)
            {
                var entries = new List<NetworkEntry<TNode, TConnection>>();
                using (var access = factory.GetEntryCollectionAccess())
                {
                    MaxEntryIndex = factory.MaxEntryIndex; 
                    foreach (var factoryEntry in access)
                    {
                        if (rules.IsValidNetworkFactoryEntry(factoryEntry))
                        {
                            if (factoryEntry.NodeFactoryEntry != null && rules.IsValidNodeFactoryEntry(factoryEntry.NodeFactoryEntry))
                            {
                                var nodeFactoryEntry = factoryEntry.NodeFactoryEntry;
                                var upperConnFactories = ConnectionFactoryEntryFilter(rules, nodeFactoryEntry, factoryEntry.UpperConnectionFactoryDict.Values);
                                var lowerConnFactories = ConnectionFactoryEntryFilter(rules, nodeFactoryEntry, factoryEntry.LowerConnectionFactoryDict.Values);

                                var node = nodeFactoryEntry.Create();
                                if (rules.IsValidNodeEntry(node))
                                {
                                    InitializeNodeEntry(node);

                                    var upperConns = ConnectionEntryFilter(rules, node, upperConnFactories.Select(e => e.Create())).ToArray();
                                    var lowerConns = ConnectionEntryFilter(rules, node, lowerConnFactories.Select(e => e.Create())).ToArray();

                                    var entry = new NetworkEntry<TNode, TConnection>(upperConns, lowerConns, node);
                                    if (rules.IsValidNetworkEntry(entry))
                                    {
                                        InitializeNetworkEntry(entry);
                                        entries.Add(entry);
                                    }
                                }
                            }
                        }
                    }
                    EntryArray = entries.ToArray();
                }
            }
        }

        protected virtual void InitializeNetworkEntry(NetworkEntry<TNode, TConnection> networkEntry)
        {
            Contract.Requires(networkEntry != null);
        }

        protected virtual void InitializeNodeEntry(NodeEntry<TNode> nodeEntry) 
        {
            Contract.Requires(nodeEntry != null);
        }

        protected virtual void InitializeConnectionEntry(NodeEntry<TNode> parent, ConnectionEntry<TConnection> connectionEntry)
        {
            Contract.Requires(parent != null);
            Contract.Requires(connectionEntry != null);
        }

        private IEnumerable<ConnectionFactoryEntry<TConnection>> ConnectionFactoryEntryFilter(
            INetworkBuildingRules<TNode, TConnection> rules,
            NodeFactoryEntry<TNode> parent,
            IEnumerable<ConnectionFactoryEntry<TConnection>> entries)
        {
            return entries.Where(e => rules.IsValidConnectionFactoryEntry(parent, e));
        }

        private IEnumerable<ConnectionEntry<TConnection>> ConnectionEntryFilter(
            INetworkBuildingRules<TNode, TConnection> rules,
            NodeEntry<TNode> parent,
            IEnumerable<ConnectionEntry<TConnection>> entries)
        {
            return entries.Where(e => rules.IsValidConnectionEntry(parent, e)).Select(e => { InitializeConnectionEntry(parent, e); return e; });
        } 

        #endregion

        #region Item Access

        public NetworkItems<TNode, TConnection> GetItems() // TODO: virtual, computation network optimization
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
