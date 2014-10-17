using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Neuroflow.Networks
{
    public struct NodeEntry<T> : IEquatable<NodeEntry<T>> where T : class
    {
        public NodeEntry(int index, T node)
        {
            Contract.Requires(index >= 0);
            Contract.Requires(node != null);

            this.index = index;
            this.node = node;
        }

        public bool IsEmpty
        {
            [Pure]
            get { return node != null; }
        }
        
        private int index;

        public int Index
        {
            get { return index; }
        }

        private T node;

        public T Node
        {
            get { return node; }
        }

        public bool Equals(NodeEntry<T> other)
        {
            return this.Index == other.Index;
        }

        public override bool Equals(object obj)
        {
            return obj is NodeEntry<T> ? Equals((NodeEntry<T>)obj) : false;
        }

        public override int GetHashCode()
        {
            return Index;
        }
    }
}
