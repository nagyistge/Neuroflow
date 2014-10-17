using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Neuroflow.Core.Collections;
using System.Diagnostics.Contracts;

namespace Neuroflow.Core.Optimizations
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = xmlns.Neuroflow, Name = "vectorFlowBatch")]
    public class VectorFlowBatch<T> : ReadOnlyArray<VectorFlow<T>>
        where T : struct
    {
        public VectorFlowBatch(VectorFlow<T> vectorFlow)
            : base(vectorFlow)
        {
            Contract.Requires(vectorFlow != null);
        }

        public VectorFlowBatch(IList<VectorFlow<T>> vectorFlowList)
            : base(vectorFlowList)
        {
            Contract.Requires(vectorFlowList != null && vectorFlowList.Count > 0);
        }

        public VectorFlowBatch(IEnumerable<VectorFlow<T>> vectorFlowCollection)
            : base(vectorFlowCollection.ToList())
        {
            Contract.Requires(vectorFlowCollection != null);

            if (ItemArray.Length == 0) throw new InvalidOperationException("VectorFlow collection is empty.");
        }
    }
}
