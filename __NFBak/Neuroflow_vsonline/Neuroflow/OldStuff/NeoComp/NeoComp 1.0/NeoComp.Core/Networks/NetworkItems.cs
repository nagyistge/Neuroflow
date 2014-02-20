using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Collections;

namespace NeoComp.Networks
{
    public struct NetworkItems<TNode, TConnection>
    {
        internal NetworkItems(IEnumerable<NodeEntry<TNode>> nodeEntries, IEnumerable<ConnectionEntry<TConnection>> connectionEntries)
        {
            Contract.Requires(nodeEntries != null);
            Contract.Requires(connectionEntries != null);

            this.nodeEntries = nodeEntries;
            this.connectionEntries = connectionEntries;
        }
        
        IEnumerable<NodeEntry<TNode>> nodeEntries;

        public IEnumerable<NodeEntry<TNode>> NodeEntries
        {
            get { return nodeEntries; }
        }

        IEnumerable<ConnectionEntry<TConnection>> connectionEntries;

        public IEnumerable<ConnectionEntry<TConnection>> ConnectionEntries
        {
            get { return connectionEntries; }
        }
    }
}
