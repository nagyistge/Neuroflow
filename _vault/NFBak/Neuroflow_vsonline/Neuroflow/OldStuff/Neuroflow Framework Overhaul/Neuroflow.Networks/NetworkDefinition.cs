using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Neuroflow.Networks
{
    public class NetworkDefinition<TNode, TConnection>
        where TNode : class
        where TConnection : class
    {
        #region Types

        struct UpperConnComparer : IComparer<ConnectionIndex>
        {
            public int Compare(ConnectionIndex x, ConnectionIndex y)
            {
                int c = x.UpperNodeIndex.CompareTo(y.UpperNodeIndex);
                if (c == 0) return x.LowerNodeIndex.CompareTo(y.LowerNodeIndex);
                return c;
            }
        }

        struct LowerConnComparer : IComparer<ConnectionIndex>
        {
            public int Compare(ConnectionIndex x, ConnectionIndex y)
            {
                int c = x.LowerNodeIndex.CompareTo(y.LowerNodeIndex);
                if (c == 0) return x.UpperNodeIndex.CompareTo(y.UpperNodeIndex);
                return c;
            }
        }

        #endregion

        #region Defs

        private static readonly ConnectionEntry<TConnection>[] emptyConnections = new ConnectionEntry<TConnection>[0];

        #endregion

        #region Props and Fields

        SortedDictionary<int, TNode> nodes = new SortedDictionary<int, TNode>();

        Dictionary<int, SortedDictionary<ConnectionIndex, TConnection>> nodeUpperConnections = new Dictionary<int, SortedDictionary<ConnectionIndex, TConnection>>();

        Dictionary<int, SortedDictionary<ConnectionIndex, TConnection>> nodeLowerConnections = new Dictionary<int, SortedDictionary<ConnectionIndex, TConnection>>();

        SortedDictionary<ConnectionIndex, TConnection> connections = new SortedDictionary<ConnectionIndex, TConnection>();

        public IEnumerable<NodeEntry<TNode>> NodeEntries
        {
            get { return nodes.Select(kvp => new NodeEntry<TNode>(kvp.Key, kvp.Value)); }
        }

        public IEnumerable<ConnectionEntry<TConnection>> ConnectionEntries
        {
            get { return connections.Select(kvp => new ConnectionEntry<TConnection>(kvp.Key, kvp.Value)); }
        }

        public int NodeCount
        {
            get { return nodes.Count; }
        }

        public int ConnectionCount
        {
            get { return connections.Count; }
        }

        public int MaxNodeIndex
        {
            get
            {
                int index = 0;
                foreach (var i in connections.Keys)
                {
                    if (i.LowerNodeIndex > index) index = i.LowerNodeIndex;
                    if (i.UpperNodeIndex > index) index = i.UpperNodeIndex;
                }
                foreach (var i in nodes.Keys)
                {
                    if (i > index) index = i;
                }
                return index;
            }
        }

        #endregion

        #region Conn CRUD

        public virtual void AddConnection(ConnectionIndex index, TConnection connection)
        {
            Contract.Requires(connection != null);
            Contract.Requires(index.LowerNodeIndex >= 0);
            Contract.Requires(index.UpperNodeIndex >= 0);

            connections[index] = connection;

            SortedDictionary<ConnectionIndex, TConnection> upperDict;
            SortedDictionary<ConnectionIndex, TConnection> lowerDict;

            if (!nodeUpperConnections.TryGetValue(index.LowerNodeIndex, out upperDict))
            {
                upperDict = new SortedDictionary<ConnectionIndex, TConnection>(new UpperConnComparer());
                nodeUpperConnections.Add(index.LowerNodeIndex, upperDict);
            }
            upperDict[index] = connection;

            if (!nodeLowerConnections.TryGetValue(index.UpperNodeIndex, out lowerDict))
            {
                lowerDict = new SortedDictionary<ConnectionIndex, TConnection>(new LowerConnComparer());
                nodeLowerConnections.Add(index.UpperNodeIndex, lowerDict);
            }
            lowerDict[index] = connection;
        }

        public virtual void RemoveConnection(ConnectionIndex index)
        {
            Contract.Requires(index.LowerNodeIndex >= 0);
            Contract.Requires(index.UpperNodeIndex >= 0);

            if (connections.Remove(index))
            {
                var upperDict = nodeUpperConnections[index.LowerNodeIndex];
                upperDict.Remove(index);
                if (upperDict.Count == 0) nodeUpperConnections.Remove(index.LowerNodeIndex);

                var lowerDict = nodeLowerConnections[index.UpperNodeIndex];
                lowerDict.Remove(index);
                if (lowerDict.Count == 0) nodeLowerConnections.Remove(index.UpperNodeIndex);
            }
        }

        public ConnectionEntry<TConnection>? GetConnectionAt(ConnectionIndex index)
        {
            Contract.Requires(index.LowerNodeIndex >= 0);
            Contract.Requires(index.UpperNodeIndex >= 0);

            TConnection conn;
            if (connections.TryGetValue(index, out conn)) return new ConnectionEntry<TConnection>(index, conn);
            return null;
        }

        public IEnumerable<ConnectionEntry<TConnection>> GetUpperConnections(int nodeIndex)
        {
            Contract.Requires(nodeIndex >= 0);

            SortedDictionary<ConnectionIndex, TConnection> upperDict;
            if (nodeUpperConnections.TryGetValue(nodeIndex, out upperDict))
            {
                return upperDict.Select(kvp => new ConnectionEntry<TConnection>(kvp.Key, kvp.Value));
            }
            return emptyConnections;
        }

        public IEnumerable<ConnectionEntry<TConnection>> GetLowerConnections(int nodeIndex)
        {
            Contract.Requires(nodeIndex >= 0);

            SortedDictionary<ConnectionIndex, TConnection> lowerDict;
            if (nodeLowerConnections.TryGetValue(nodeIndex, out lowerDict))
            {
                return lowerDict.Select(kvp => new ConnectionEntry<TConnection>(kvp.Key, kvp.Value));
            }
            return emptyConnections;
        }

        #endregion

        #region Node CRUD

        public virtual void AddNode(int index, TNode node)
        {
            Contract.Requires(node != null);
            Contract.Requires(index >= 0);

            nodes[index] = node;
        }

        public virtual void RemoveNode(int index)
        {
            Contract.Requires(index >= 0);

            nodes.Remove(index);
        }

        public NodeEntry<TNode>? GetNodeAt(int index)
        {
            Contract.Requires(index >= 0);

            TNode conn;
            if (nodes.TryGetValue(index, out conn)) return new NodeEntry<TNode>(index, conn);
            return null;
        }

        #endregion
    }
}
