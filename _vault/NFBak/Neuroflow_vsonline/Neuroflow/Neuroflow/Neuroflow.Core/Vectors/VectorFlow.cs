using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Diagnostics.Contracts;
using System.Collections.ObjectModel;
using Neuroflow.Core.Serialization;

namespace Neuroflow.Core.Vectors
{
    [Serializable, Known, DataContract(Namespace = xmlns.Neuroflow)]
    public class VectorFlow<T>
        where T : struct
    {
        public VectorFlow(int index, VectorFlowEntry<T> entry)
        {
            Contract.Requires(index >= 0);
            Contract.Requires(entry != null);

            Index = index;
            entries = new[] { entry };
        }

        public VectorFlow(int index, VectorFlowEntry<T>[] entries)
        {
            Contract.Requires(index >= 0);
            Contract.Requires(entries != null && entries.Length > 0);

            Index = index;
            this.entries = entries;
        }

        [DataMember]
        public int Index { get; private set; }

        [DataMember(Name = "Entries")]
        internal VectorFlowEntry<T>[] entries;

        public ReadOnlyCollection<VectorFlowEntry<T>> Entries
        {
            get { return Array.AsReadOnly(entries); }
        }
    }
}
