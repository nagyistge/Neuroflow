using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core;
using System.Diagnostics.Contracts;

namespace Neuroflow.Core.Networks
{
    public class ConnectionFactoryEntry<T>
    {
        internal ConnectionFactoryEntry(ConnectionIndex index, IFactory<T> connectionFactory)
        {
            Contract.Requires(!object.ReferenceEquals(connectionFactory, null));

            Index = index;
            ConnectionFactory = connectionFactory;
        }

        ConnectionEntry<T> currentEntry;

        public ConnectionIndex Index { get; private set; }

        public IFactory<T> ConnectionFactory { get; private set; }

        internal ConnectionEntry<T> Create()
        {
            if (currentEntry != null) return currentEntry; 
            
            T conn;
            try
            {
                conn = ConnectionFactory.Create();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Cannot create connection. See inner exception for details.", ex);
            }
            
            if (object.ReferenceEquals(conn, null)) throw new InvalidOperationException("Factored connection is null.");

            currentEntry = new ConnectionEntry<T>(Index, conn);

            return currentEntry;
        }

        internal void Reset()
        {
            currentEntry = null;
        }
    }
}
