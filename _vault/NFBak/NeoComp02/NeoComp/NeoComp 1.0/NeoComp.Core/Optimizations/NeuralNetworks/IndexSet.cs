using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimizations.NeuralNetworks
{
    [Serializable]
    [CollectionDataContract(Name = "indexSet", ItemName = "index", ValueName = "value", Namespace = xmlns.NeoCompNS)]
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
    }
}
