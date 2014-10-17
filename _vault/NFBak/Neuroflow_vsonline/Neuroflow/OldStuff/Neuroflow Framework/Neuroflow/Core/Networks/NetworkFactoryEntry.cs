using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core;
using System.Diagnostics.Contracts;

namespace Neuroflow.Core.Networks
{
    public sealed class NetworkFactoryEntry<TNode, TConnection>
    {
        static readonly IEnumerable<ConnectionFactoryEntry<TConnection>> emptyConns = new ConnectionFactoryEntry<TConnection>[0];
        
        internal NetworkFactoryEntry(int index)
        {
            Contract.Requires(index >= 0);

            Index = index;
        }

        internal NetworkFactoryEntry(NodeFactoryEntry<TNode> nodeFactoryEntry)
        {
            Contract.Requires(nodeFactoryEntry != null);
            
            Index = nodeFactoryEntry.Index;
            this.nodeFactoryEntry = nodeFactoryEntry;
        }

        public bool IsEmpty
        {
            get
            {
                return NodeFactoryEntry == null &&
                    (upperConnectionFactoryDict == null || upperConnectionFactoryDict.Count == 0) &&
                    (lowerConnectionFactoryDict == null || lowerConnectionFactoryDict.Count == 0);
            }
        }

        public int Index { get; private set; }

        NodeFactoryEntry<TNode> nodeFactoryEntry;

        public NodeFactoryEntry<TNode> NodeFactoryEntry
        {
            get { return nodeFactoryEntry; }
            internal set
            {
                Contract.Requires(value == null || value.Index == Index);

                nodeFactoryEntry = value;
            }
        }

        SortedDictionary<int, ConnectionFactoryEntry<TConnection>> upperConnectionFactoryDict;

        internal SortedDictionary<int, ConnectionFactoryEntry<TConnection>> UpperConnectionFactoryDict
        {
            get { return upperConnectionFactoryDict ?? (upperConnectionFactoryDict = new SortedDictionary<int, ConnectionFactoryEntry<TConnection>>()); }
        }

        SortedDictionary<int, ConnectionFactoryEntry<TConnection>> lowerConnectionFactoryDict;

        internal SortedDictionary<int, ConnectionFactoryEntry<TConnection>> LowerConnectionFactoryDict
        {
            get { return lowerConnectionFactoryDict ?? (lowerConnectionFactoryDict = new SortedDictionary<int, ConnectionFactoryEntry<TConnection>>()); }
        }

        public IEnumerable<ConnectionFactoryEntry<TConnection>> UpperConnectionFactoryEntries
        {
            get { return upperConnectionFactoryDict == null ? emptyConns : upperConnectionFactoryDict.Values; }
        }

        public IEnumerable<ConnectionFactoryEntry<TConnection>> LowerConnectionFactoryEntries
        {
            get { return lowerConnectionFactoryDict == null ? emptyConns : lowerConnectionFactoryDict.Values; }
        }

        internal void Reset()
        {
            if (NodeFactoryEntry != null) NodeFactoryEntry.Reset();
            if (upperConnectionFactoryDict != null) foreach (var uce in upperConnectionFactoryDict.Values) uce.Reset();
        }
    }
}
