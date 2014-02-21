using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Neuroflow.Networks
{
    public struct ConnectionEntry<T> : IEquatable<ConnectionEntry<T>> where T : class
    {
        public ConnectionEntry(ConnectionIndex index, T connection)
        {
            Contract.Requires(connection != null);

            this.index = index;
            this.connection = connection;
        }

        public bool IsEmpty
        {
            [Pure]
            get { return connection != null; }
        }

        private ConnectionIndex index;

        public ConnectionIndex Index
        {
            get { return index; }
        }

        private T connection;

        public T Connection
        {
            get { return connection; }
        }

        public bool Equals(ConnectionEntry<T> other)
        {
            return this.Index == other.Index;
        }

        public override bool Equals(object obj)
        {
            return obj is ConnectionEntry<T> ? Equals((ConnectionEntry<T>)obj) : false;
        }

        public override int GetHashCode()
        {
            return Index.GetHashCode();
        }
    }
}
