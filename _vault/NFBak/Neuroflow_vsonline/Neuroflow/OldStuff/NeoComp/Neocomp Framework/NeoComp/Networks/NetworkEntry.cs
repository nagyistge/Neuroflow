using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace NeoComp.Networks
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = NeoComp.xmlns, Name = "netEntry")]
    public sealed class NetworkEntry<TNode, TConnection>
    {
        internal NetworkEntry(
            ConnectionEntry<TConnection>[] upperConnectionEntryArray,
            ConnectionEntry<TConnection>[] lowerConnectionEntryArray, 
            NodeEntry<TNode> nodeEntry)
        {
            Contract.Requires(upperConnectionEntryArray != null);
            Contract.Requires(lowerConnectionEntryArray != null);
            Contract.Requires(nodeEntry != null);
            

            UpperConnectionEntryArray = upperConnectionEntryArray;
            LowerConnectionEntryArray = lowerConnectionEntryArray;
            NodeEntry = nodeEntry;
        }

        [DataMember(Name = "upper", Order = 1)]
        internal ConnectionEntry<TConnection>[] UpperConnectionEntryArray { get; private set; }

        [DataMember(Name = "lower", Order = 2)]
        internal ConnectionEntry<TConnection>[] LowerConnectionEntryArray { get; private set; }

        public ReadOnlyCollection<ConnectionEntry<TConnection>> UpperConnectionEntries
        {
            get { return new ReadOnlyCollection<ConnectionEntry<TConnection>>(((IList<ConnectionEntry<TConnection>>)UpperConnectionEntryArray)); }
        }

        public ReadOnlyCollection<ConnectionEntry<TConnection>> LowerConnectionEntries
        {
            get { return new ReadOnlyCollection<ConnectionEntry<TConnection>>(((IList<ConnectionEntry<TConnection>>)LowerConnectionEntryArray)); }
        }

        [DataMember(Name = "node", Order = 0)]
        public NodeEntry<TNode> NodeEntry { get; private set; }
    }
}
