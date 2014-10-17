using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Diagnostics.Contracts;
using System.Threading;

namespace NeoComp.Networks
{
    public abstract class NetworkFactory<TNode, TConnection> : SynchronizedObject
    {
        #region Constructors

        protected NetworkFactory()
        {
        }

        protected NetworkFactory(Network<TNode, TConnection> network)
        {
            Build(network);
        }

        #endregion
        
        #region Fields And Properties

        SortedDictionary<int, NetworkFactoryEntry<TNode, TConnection>> entries = new SortedDictionary<int, NetworkFactoryEntry<TNode, TConnection>>();

        int? maxEntryIndex;
        
        public int MaxEntryIndex
        {
            get 
            {
                if (!maxEntryIndex.HasValue)
                {
                    lock (SyncRoot)
                    {
                        if (!maxEntryIndex.HasValue)
                        {
                            maxEntryIndex = entries.Keys.LastOrDefault(); 
                        }
                    }
                }
                return maxEntryIndex.Value;
            }
        }

        int? nodeFactoryCount;

        public int NodeFactoryCount
        {
            get
            {
                if (!nodeFactoryCount.HasValue)
                {
                    lock (SyncRoot)
                    {
                        if (!nodeFactoryCount.HasValue)
                        {
                            nodeFactoryCount = entries.Values.Count(n => n.NodeFactoryEntry != null);
                        }
                    }
                }
                return nodeFactoryCount.Value;
            }
        }

        int? connectionFactoryCount;

        public int ConnectionFactoryCount
        {
            get
            {
                if (!connectionFactoryCount.HasValue)
                {
                    lock (SyncRoot)
                    {
                        if (!connectionFactoryCount.HasValue)
                        {
                            connectionFactoryCount = entries.Values.Select(e => e.LowerConnectionFactoryDict.Count).Sum();
                        }
                    }
                }
                return connectionFactoryCount.Value;
            }
        }

        #endregion

        #region Build From Network

        private void Build(Network<TNode, TConnection> network)
        {
            var sync = network as ISynchronized;
            if (sync != null) Monitor.Enter(sync.SyncRoot);
            try
            {
                foreach (var entry in network.EntryArray)
                {
                    AddNodeEntry(entry.NodeEntry);
                    foreach (var ce in entry.UpperConnectionEntryArray) AddConnectionEntry(ce);
                    foreach (var ce in entry.LowerConnectionEntryArray) AddConnectionEntry(ce);
                }
            }
            finally
            {
                if (sync != null) Monitor.Exit(sync.SyncRoot);
            }
        } 

        #endregion

        #region Entry Management

        private NetworkFactoryEntry<TNode, TConnection> GetOrCreateEntry(int index)
        {
            NetworkFactoryEntry<TNode, TConnection> entry;
            if (entries.TryGetValue(index, out entry))
            {
                return entry;
            }
            else
            {
                entry = new NetworkFactoryEntry<TNode, TConnection>(index);
                entries.Add(index, entry);
                return entry;
            }
        }

        private NetworkFactoryEntry<TNode, TConnection> GetEntry(int index)
        {
            NetworkFactoryEntry<TNode, TConnection> entry;
            if (entries.TryGetValue(index, out entry))
            {
                return entry;
            }
            else
            {
                return null;
            }
        }

        private void EntriesChanged()
        {
            nodeFactoryCount = null;
            connectionFactoryCount = null;
            maxEntryIndex = null;
        }

        #endregion

        #region Node Management

        private void AddNodeEntry(NodeEntry<TNode> nodeEntry)
        {
            NetworkFactoryEntry<TNode, TConnection> entry;
            if (entries.TryGetValue(nodeEntry.Index, out entry))
            {
                entry.NodeFactoryEntry = new ActiveNodeFactoryEntry<TNode>(nodeEntry);
            }
            else
            {
                entries.Add(nodeEntry.Index, new NetworkFactoryEntry<TNode, TConnection>(new ActiveNodeFactoryEntry<TNode>(nodeEntry)));
            }
        }

        public bool TryAddNodeFactory(int index, IFactory<TNode> nodeFactory)
        {
            Contract.Requires(index >= 0);
            Contract.Requires(nodeFactory != null);

            lock (SyncRoot)
            {
                NetworkFactoryEntry<TNode, TConnection> entry;
                if (entries.TryGetValue(index, out entry))
                {
                    if (entry.NodeFactoryEntry != null) return false;
                    entry.NodeFactoryEntry = new NodeFactoryEntry<TNode>(index, nodeFactory);
                }
                else
                {
                    entries.Add(index, new NetworkFactoryEntry<TNode, TConnection>(new NodeFactoryEntry<TNode>(index, nodeFactory)));
                }
                EntriesChanged();
                return true;
            }
        }

        public void AddNodeFactory(int index, IFactory<TNode> nodeFactory)
        {
            Contract.Requires(index >= 0);
            Contract.Requires(nodeFactory != null); 
            
            if (!TryAddNodeFactory(index, nodeFactory)) throw new ArgumentException("nodeIndex", "Node already exists.");
        }

        public bool RemoveNodeFactory(int index)
        {
            Contract.Requires(index >= 0); 

            lock (SyncRoot)
            {
                NetworkFactoryEntry<TNode, TConnection> entry;
                if (entries.TryGetValue(index, out entry))
                {
                    if (entry.NodeFactoryEntry != null)
                    {
                        entry.NodeFactoryEntry = null;
                        if (entry.IsEmpty) entries.Remove(index);
                        EntriesChanged();
                        return true;
                    }
                }
                return false;
            }
        }

        #endregion

        #region Connection Management

        private void AddConnectionEntry(ConnectionEntry<TConnection> connectionEntry)
        {
            int uIdx = connectionEntry.Index.UpperNodeIndex;
            int lIdx = connectionEntry.Index.LowerNodeIndex;
            var upperEntry = GetOrCreateEntry(uIdx);
            var lowerEntry = GetOrCreateEntry(lIdx);
            if (upperEntry.LowerConnectionFactoryDict.ContainsKey(lIdx))
            {
                if (upperEntry.IsEmpty) entries.Remove(uIdx);
                if (lowerEntry.IsEmpty) entries.Remove(lIdx);
            }
            else
            {
                var conn = new ActiveConnectionFactoryEntry<TConnection>(connectionEntry);
                upperEntry.LowerConnectionFactoryDict[lIdx] = conn;
                lowerEntry.UpperConnectionFactoryDict[uIdx] = conn;
            }
        }

        public bool TryAddConnectionFactory(ConnectionIndex index, IFactory<TConnection> connectionFactory)
        {
            Contract.Requires(connectionFactory != null);

            lock (SyncRoot)
            {
                int uIdx = index.UpperNodeIndex;
                int lIdx = index.LowerNodeIndex;
                var upperEntry = GetOrCreateEntry(uIdx);
                var lowerEntry = GetOrCreateEntry(lIdx);
                if (upperEntry.LowerConnectionFactoryDict.ContainsKey(lIdx))
                {
                    if (upperEntry.IsEmpty) entries.Remove(uIdx);
                    if (lowerEntry.IsEmpty) entries.Remove(lIdx);
                    return false;
                }
                else
                {
                    var conn = new ConnectionFactoryEntry<TConnection>(index, connectionFactory);
                    upperEntry.LowerConnectionFactoryDict[lIdx] = conn;
                    lowerEntry.UpperConnectionFactoryDict[uIdx] = conn;
                    EntriesChanged();
                    return true;
                }
            }
        }

        public void AddConnectionFactory(ConnectionIndex index, IFactory<TConnection> connectionFactory)
        {
            Contract.Requires(connectionFactory != null);

            if (!TryAddConnectionFactory(index, connectionFactory)) throw new ArgumentException("index", "Connection already exists.");
        }

        public bool RemoveConnection(ConnectionIndex index)
        {
            lock (SyncRoot)
            {
                int uIdx = index.UpperNodeIndex;
                int lIdx = index.LowerNodeIndex;
                NetworkFactoryEntry<TNode, TConnection> upperEntry, lowerEntry;
                if ((upperEntry = GetEntry(uIdx)) != null && (lowerEntry = GetEntry(lIdx)) != null)
                {
                    ConnectionFactoryEntry<TConnection> conn;
                    if (upperEntry.LowerConnectionFactoryDict.TryGetValue(lIdx, out conn))
                    {
                        bool r1 = upperEntry.LowerConnectionFactoryDict.Remove(lIdx);
                        bool r2 = lowerEntry.UpperConnectionFactoryDict.Remove(uIdx);
                        if (r1 && r2)
                        {
                            if (upperEntry.IsEmpty) entries.Remove(uIdx);
                            if (lowerEntry.IsEmpty) entries.Remove(lIdx);
                            EntriesChanged();
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        #endregion

        #region Access

        internal NetworkFactoryEntryCollectionAccess<TNode, TConnection> GetEntryCollectionAccess()
        {
            return new NetworkFactoryEntryCollectionAccess<TNode, TConnection>(this.entries.Values);
        }

        #endregion
    }
}
