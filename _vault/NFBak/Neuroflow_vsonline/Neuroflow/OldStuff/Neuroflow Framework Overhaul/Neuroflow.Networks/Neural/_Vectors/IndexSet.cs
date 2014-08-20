using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Diagnostics.Contracts;
using Neuroflow.Core.Serialization;

namespace Neuroflow.Networks.Neural
{
    [Serializable, Known, CollectionDataContract(Namespace = xmlns.Neuroflow, ItemName = "Index")]
    public sealed class IndexSet : HashSet<int>
    {
        public IndexSet()
        {
        }

        public IndexSet(IEnumerable<int> indexes)
            : base(indexes)
        {
            Contract.Requires(indexes != null);
        }

        private IndexSet(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
