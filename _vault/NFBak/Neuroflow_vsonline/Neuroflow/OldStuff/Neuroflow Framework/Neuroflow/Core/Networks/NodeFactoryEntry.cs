using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core;
using System.Diagnostics.Contracts;

namespace Neuroflow.Core.Networks
{
    public class NodeFactoryEntry<T>
    {
        internal NodeFactoryEntry(int index, IFactory<T> nodeFactory)
        {
            Contract.Requires(index >= 0);
            Contract.Requires(nodeFactory != null);

            Index = index;
            NodeFactory = nodeFactory;
        }

        NodeEntry<T> currentEntry;
        
        public int Index { get; private set; }

        public IFactory<T> NodeFactory { get; internal set; }

        internal NodeEntry<T> Create()
        {
            if (currentEntry != null) return currentEntry;

            T node;
            try
            {
                node = NodeFactory.Create();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Cannot create node. See inner exception for details.", ex);
            }
            
            if (object.ReferenceEquals(node, null)) throw new InvalidOperationException("Factored node is null.");

            currentEntry = new NodeEntry<T>(Index, node);
            return currentEntry;
        }

        internal void Reset()
        {
            currentEntry = null;
        }
    }
}
