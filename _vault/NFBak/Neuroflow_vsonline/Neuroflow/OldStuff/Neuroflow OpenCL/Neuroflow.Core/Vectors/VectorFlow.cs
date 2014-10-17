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
        public VectorFlow(VectorFlowEntry<T> entry)
        {
            Contract.Requires(entry != null);

            entries = new[] { entry };
        }

        public VectorFlow(VectorFlowEntry<T>[] entries)
        {
            Contract.Requires(entries != null && entries.Length > 0);

            this.entries = entries;
        }

        [DataMember(Name = "Entries")]
        internal VectorFlowEntry<T>[] entries;

        public ReadOnlyCollection<VectorFlowEntry<T>> Entries
        {
            get { return Array.AsReadOnly(entries); }
        }
    }
}
