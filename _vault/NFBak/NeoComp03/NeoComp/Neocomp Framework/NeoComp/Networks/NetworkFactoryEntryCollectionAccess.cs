using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Networks
{
    internal sealed class NetworkFactoryEntryCollectionAccess<TNode, TConnection> : IEnumerable<NetworkFactoryEntry<TNode, TConnection>>, IDisposable
    {
        internal NetworkFactoryEntryCollectionAccess(IEnumerable<NetworkFactoryEntry<TNode, TConnection>> entries)
        {
            Contract.Requires(entries != null);

            this.entries = entries;
        }

        IEnumerable<NetworkFactoryEntry<TNode, TConnection>> entries;

        private void ResetEntries()
        {
            foreach (var entry in entries)
            {
                entry.Reset();
            }
        }

        bool disposed;

        void IDisposable.Dispose()
        {
            if (!disposed)
            {
                ResetEntries();
                disposed = true;
            }
        }

        public IEnumerator<NetworkFactoryEntry<TNode, TConnection>> GetEnumerator()
        {
            return entries.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
