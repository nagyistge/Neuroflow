using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using NeoComp.Collections;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimizations
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = xmlns.NeoCompNS, Name = "vectorFlow")]
    public class VectorFlow<T> : ReadOnlyArray<VectorFlowEntry<T>>
        where T : struct
    {
        public VectorFlow(VectorFlowEntry<T> entry)
            : base(entry)
        {
            Contract.Requires(entry != null);
        }

        public VectorFlow(IList<VectorFlowEntry<T>> entryList)
            : base(entryList)
        {
            Contract.Requires(entryList != null && entryList.Count > 0);
        }

        public VectorFlow(IEnumerable<VectorFlowEntry<T>> entryColl)
            : base(entryColl)
        {
            Contract.Requires(entryColl != null);

            if (ItemArray.Length == 0) throw new InvalidOperationException("Entry collection is empty.");
        }
    }
}
