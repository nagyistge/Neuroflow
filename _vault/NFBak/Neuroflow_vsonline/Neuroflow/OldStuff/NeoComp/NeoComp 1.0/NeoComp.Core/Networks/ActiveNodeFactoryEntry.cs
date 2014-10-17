using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp;
using System.Diagnostics.Contracts;

namespace NeoComp.Networks
{
    public sealed class ActiveNodeFactoryEntry<T> : NodeFactoryEntry<T>
    {
        internal ActiveNodeFactoryEntry(NodeEntry<T> activeNodeEntry)
            : base(activeNodeEntry.Index, new ClonerFactory<T>(activeNodeEntry.Node))
        {
            Contract.Requires(activeNodeEntry != null);

            Node = activeNodeEntry.Node;
        }

        public T Node { get; private set; }
    }
}
