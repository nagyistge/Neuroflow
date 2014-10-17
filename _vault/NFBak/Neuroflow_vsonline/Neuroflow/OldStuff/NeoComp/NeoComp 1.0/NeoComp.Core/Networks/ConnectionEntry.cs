using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;

namespace NeoComp.Networks
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = xmlns.NeoCompNS, Name = "connEntry")]
    public sealed class ConnectionEntry<T>
    {
        internal ConnectionEntry(ConnectionIndex index, T connection)
        {
            Contract.Requires(!object.ReferenceEquals(connection, null));

            Index = index;
            Connection = connection;
        }

        [DataMember(Name = "index")]
        public ConnectionIndex Index { get; private set; }

        [DataMember(Name = "connection")]
        public T Connection { get; private set; }
    }
}
