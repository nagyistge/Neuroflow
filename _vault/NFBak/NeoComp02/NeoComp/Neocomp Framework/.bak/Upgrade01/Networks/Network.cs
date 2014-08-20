using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Collections;

namespace NeoComp.Networks
{
    // TODO : Covariance!
    public interface INetwork : ISynchronized
    {
        int Size { get; }

        #region Connections

        int ConnectionCount { get; }

        Connection this[ConnectionIndex index] { get; }

        IEnumerable GetConnections();

        bool TryAddConnection(ConnectionIndex index, Connection connection);

        void AddConnection(ConnectionIndex index, Connection connection);

        bool RemoveConnection(ConnectionIndex index);

        #endregion

        #region Nodes

        int NodeCount { get; }

        Node this[int nodeIndex] { get; }

        IEnumerable GetNodes();

        IList ConnectedNodes { get; }

        void ReleaseContext();

        void AddNode(int nodeIndex, Node node);

        bool TryAddNode(int nodeIndex, Node node);

        bool RemoveNode(int nodeIndex);

        bool RemoveNode(Node node); 

        #endregion
    }
    
    public class Network<TConnection, TNode> : SynchronizedObject, INetwork
        where TConnection : Connection
        where TNode : Node
    {
        #region Node Entry Class

        private class NodeEntry
        {
            internal NodeEntry() { }

            internal NodeEntry(TNode node)
            {
                Node = node;
            }

            internal bool IsEmpty
            {
                get
                {
                    return Node == null &&
                        (upperConnections == null || upperConnections.Count == 0) &&
                        (lowerConnections == null || lowerConnections.Count == 0);
                }
            }

            internal TNode Node { get; set; }

            SortedDictionary<int, TConnection> upperConnections;

            internal SortedDictionary<int, TConnection> UpperConnections
            {
                get { return upperConnections ?? (upperConnections = new SortedDictionary<int, TConnection>()); }
            }

            SortedDictionary<int, TConnection> lowerConnections;

            internal SortedDictionary<int, TConnection> LowerConnections
            {
                get { return lowerConnections ?? (lowerConnections = new SortedDictionary<int, TConnection>()); }
            }
        }

        #endregion

        #region Fields

        SortedDictionary<int, NodeEntry> entries = new SortedDictionary<int, NodeEntry>();

        #endregion

        #region Properties

        int? size = 0;

        public int Size
        {
            get
            {
                lock (SyncRoot)
                {
                    if (!size.HasValue)
                    {
                        size = entries.Keys.LastOrDefault();
                    }
                    return size.Value;
                }
            }
        }

        public int ConnectionCount
        {
            get
            {
                lock (SyncRoot) return entries.Values.Select(e => e.LowerConnections.Count).Sum();
            }
        }

        public TConnection this[ConnectionIndex index]
        {
            get
            {
                lock (SyncRoot)
                {
                    NodeEntry entry;
                    TConnection conn;
                    if (entries.TryGetValue(index.UpperNodeIndex, out entry) && entry.LowerConnections.TryGetValue(index.LowerNodeIndex, out conn))
                    {
                        return conn;
                    }
                    return null;
                }
            }
        }

        public int NodeCount
        {
            get { lock (SyncRoot) return entries.Values.Select(e => e.Node).Count(n => n != null); }
        }

        public TNode this[int nodeIndex]
        {
            get
            {
                if (nodeIndex < 0) throw new ArgumentOutOfRangeException("nodeIndex");
                lock (SyncRoot)
                {
                    NodeEntry entry;
                    if (entries.TryGetValue(nodeIndex, out entry)) return entry.Node;
                    return default(TNode);
                }
            }
        }

        ConnectedNodeCollection<TConnection, TNode> connectedNodes;

        public ConnectedNodeCollection<TConnection, TNode> ConnectedNodes
        {
            get
            {
                lock (SyncRoot)
                {
                    EnsureConnectedNodes();
                    return connectedNodes;
                }
            }
        }

        #endregion

        #region Entry

        private void AddEntry(int index, NodeEntry entry)
        {
            entries.Add(index, entry);
            if (size.HasValue && index > size.Value) size = index;
        }

        private void RemoveEntry(int index)
        {
            entries.Remove(index);
            size = null;
        }

        private NodeEntry GetOrCreateEntry(int index)
        {
            NodeEntry entry;
            if (entries.TryGetValue(index, out entry))
            {
                return entry;
            }
            else
            {
                entry = new NodeEntry();
                AddEntry(index, entry);
                return entry;
            }
        }

        private NodeEntry GetEntry(int index)
        {
            NodeEntry entry;
            if (entries.TryGetValue(index, out entry))
            {
                return entry;
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region Node Management

        public bool TryAddNode(int nodeIndex, TNode node)
        {
            if (nodeIndex < 0) throw new ArgumentOutOfRangeException("nodeIndex");
            Args.IsNotNull(node, "node");

            lock (SyncRoot)
            {
                NodeEntry entry;
                if (entries.TryGetValue(nodeIndex, out entry))
                {
                    if (entry.Node != null) return false;
                    entry.Node = node;
                }
                else
                {
                    AddEntry(nodeIndex, new NodeEntry(node));
                }
                node.Index = nodeIndex;
                NodeAdded(node);
                return true;
            }
        }

        public void AddNode(int nodeIndex, TNode node)
        {
            if (!TryAddNode(nodeIndex, node)) throw new ArgumentException("nodeIndex", "Node already exists.");
        }

        public bool RemoveNode(int nodeIndex)
        {
            if (nodeIndex < 0) throw new ArgumentOutOfRangeException("nodeIndex");

            lock (SyncRoot)
            {
                NodeEntry entry;
                if (entries.TryGetValue(nodeIndex, out entry))
                {
                    if (entry.Node != null)
                    {
                        var removedNode = entry.Node;
                        entry.Node = null;
                        if (entry.IsEmpty)
                        {
                            RemoveEntry(nodeIndex);
                        }
                        NodeRemoved(removedNode);
                        return true;
                    }
                }
                return false;
            }
        }

        public bool RemoveNode(TNode node)
        {
            Args.IsNotNull(node, "node");

            lock (SyncRoot)
            {
                NodeEntry entry;
                if (entries.TryGetValue(node.Index, out entry))
                {
                    if (entry.Node == node)
                    {
                        entry.Node = null;
                        if (entry.IsEmpty)
                        {
                            RemoveEntry(node.Index);
                        }
                        NodeRemoved(node);
                        return true;
                    }
                }
                return false;
            }
        }

        protected virtual void NodeAdded(TNode node)
        {
            ReleaseContext();
        }

        protected virtual void NodeRemoved(TNode node) 
        {
            ReleaseContext();
        }

        #endregion

        #region Connection Management

        public bool TryAddConnection(ConnectionIndex index, TConnection connection)
        {
            connection.IsNotNull("connection");

            lock (SyncRoot)
            {
                int uIdx = index.UpperNodeIndex;
                int lIdx = index.LowerNodeIndex;
                var upperEntry = GetOrCreateEntry(uIdx);
                var lowerEntry = GetOrCreateEntry(lIdx);
                if (upperEntry.LowerConnections.ContainsKey(lIdx))
                {
                    if (upperEntry.IsEmpty) RemoveEntry(uIdx);
                    if (lowerEntry.IsEmpty) RemoveEntry(lIdx);
                    return false;
                }
                else
                {
                    upperEntry.LowerConnections[lIdx] = connection;
                    lowerEntry.UpperConnections[uIdx] = connection;
                    connection.Index = index;
                    ConnectionAdded(connection);
                    return true;
                }
            }
        }

        public void AddConnection(ConnectionIndex index, TConnection connection)
        {
            if (!TryAddConnection(index, connection)) throw new ArgumentException("index", "Connection already exists.");
        }

        public bool RemoveConnection(ConnectionIndex index)
        {
            lock (SyncRoot)
            {
                int uIdx = index.UpperNodeIndex;
                int lIdx = index.LowerNodeIndex;
                NodeEntry upperEntry, lowerEntry;
                if ((upperEntry = GetEntry(uIdx)) != null && (lowerEntry = GetEntry(lIdx)) != null)
                {
                    TConnection conn;
                    if (upperEntry.LowerConnections.TryGetValue(lIdx, out conn))
                    {
                        bool r1 = upperEntry.LowerConnections.Remove(lIdx);
                        bool r2 = lowerEntry.UpperConnections.Remove(uIdx);
                        if (r1 && r2)
                        {
                            if (upperEntry.IsEmpty) RemoveEntry(uIdx);
                            if (lowerEntry.IsEmpty) RemoveEntry(lIdx);
                            ConnectionRemoved(conn);
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        protected virtual void ConnectionAdded(TConnection connection)
        {
            ReleaseContext();
        }

        protected virtual void ConnectionRemoved(TConnection connection)
        {
            ReleaseContext();
        }        

        #endregion

        #region Access

        public IEnumerable<TConnection> GetConnections()
        {
            lock (SyncRoot)
            {
                foreach (var entry in entries.Values)
                    foreach (var conn in entry.UpperConnections.Values)
                        yield return conn;
            }
        }

        public IEnumerable<TNode> GetNodes()
        {
            lock (SyncRoot)
            {
                foreach (var n in entries.Values.Select(e => e.Node).Where(n => n != null)) yield return n;
            }
        }

        #endregion

        #region Node Generation

        private void EnsureConnectedNodes()
        {
            if (connectedNodes == null) connectedNodes = GenerateConnectedNodes();
        }

        protected virtual bool IsValid(TNode node)
        {
            return node.Index <= Size;
        }

        protected virtual bool IsValid(ConnectedNode<TConnection, TNode> connectedNode)
        {
            return true;
        }

        protected virtual bool IsValid(TConnection connection)
        {
            return true;
        }

        protected virtual ConnectedNodeCollection<TConnection, TNode> GenerateConnectedNodes()
        {
            var connectedNodes = from entry in entries.Values
                                 where entry.Node != null && IsValid(entry.Node)
                                 let cn = new ConnectedNode<TConnection, TNode>(
                                            entry.UpperConnections.Values.Where(c => IsValid(c)),
                                            entry.LowerConnections.Values.Where(c => IsValid(c)
                                         ),
                                     entry.Node)
                                 where IsValid(cn)
                                 select cn;

            return new ConnectedNodeCollection<TConnection, TNode>(connectedNodes.ToArray());
        }

        public void EnsureContext()
        {
            lock (SyncRoot) EnsureContextItems();
        }

        protected virtual void EnsureContextItems()
        {
            EnsureConnectedNodes();
        }

        public void ReleaseContext()
        {
            lock (SyncRoot) ReleaseContextItems();
        }

        protected virtual void ReleaseContextItems()
        {
            connectedNodes = null;
        }

        #endregion

        #region INetwork Members

        Connection INetwork.this[ConnectionIndex index]
        {
            get { return this[index]; }
        }

        IEnumerable INetwork.GetConnections()
        {
            return GetConnections();
        }

        bool INetwork.TryAddConnection(ConnectionIndex index, Connection connection)
        {
            return TryAddConnection(index, connection.Cast<TConnection>("connection"));
        }

        void INetwork.AddConnection(ConnectionIndex index, Connection connection)
        {
            AddConnection(index, connection.Cast<TConnection>("connection"));
        }

        Node INetwork.this[int nodeIndex]
        {
            get { return this[nodeIndex]; }
        }

        IList INetwork.ConnectedNodes
        {
            get { return ConnectedNodes; }
        }

        bool INetwork.TryAddNode(int nodeIndex, Node node)
        {
            return TryAddNode(nodeIndex, Args.CastAs<TNode>(node, "node"));
        }

        void INetwork.AddNode(int nodeIndex, Node node)
        {
            AddNode(nodeIndex, Args.CastAs<TNode>(node, "node"));
        }

        IEnumerable INetwork.GetNodes()
        {
            return GetNodes();
        }

        bool INetwork.RemoveNode(Node node)
        {
            return RemoveNode(Args.CastAs<TNode>(node, "node"));
        }

        #endregion
    }
}
