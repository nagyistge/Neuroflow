using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;

namespace Neuroflow.Core.Networks
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = xmlns.Neuroflow, Name = "nodeEntry")]
    public sealed class NodeEntry<T>
    {
        internal NodeEntry(int index, T node)
        {
            Contract.Requires(index >= 0);
            Contract.Requires(!object.ReferenceEquals(node, null));

            Index = index;
            Node = node;
        }

        [DataMember(Name = "index")]
        public int Index { get; private set; }

        [DataMember(Name = "node")]
        public T Node { get; internal set; }
    }
}
