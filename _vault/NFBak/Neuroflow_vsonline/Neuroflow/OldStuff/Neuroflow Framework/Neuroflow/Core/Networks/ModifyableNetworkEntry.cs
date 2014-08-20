using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace Neuroflow.Core.Networks
{
    public sealed class ModifyableNetworkEntry<TNode, TConnection>
    {
        #region Contruct and Create

        internal ModifyableNetworkEntry(ModifyableNetworkFactoryEntry<TNode, TConnection> factoryEntry)
        {
            Contract.Requires(factoryEntry != null);
            Contract.Requires(factoryEntry.NodeFactoryEntry != null);

            NodeEntry = factoryEntry.NodeFactoryEntry.Create();
            UpperConnectionEntries = new NoAddCollection<ConnectionEntry<TConnection>>(factoryEntry.UpperConnectionFactoryEntries.Select(f => f.Create()).ToList());
            LowerConnectionEntries = new NoAddCollection<ConnectionEntry<TConnection>>(factoryEntry.LowerConnectionFactoryEntries.Select(f => f.Create()).ToList());
        }

        #endregion

        #region Properties

        public NodeEntry<TNode> NodeEntry { get; private set; }

        public TNode Node
        {
            get { return NodeEntry.Node; }
            set
            {
                Contract.Requires(value != null);
                NodeEntry.Node = value;
            }
        }

        public IList<ConnectionEntry<TConnection>> UpperConnectionEntries { get; private set; }

        public IList<ConnectionEntry<TConnection>> LowerConnectionEntries { get; private set; }

        #endregion

        #region CreateNE

        internal NetworkEntry<TNode, TConnection> CreateNetworkEntry()
        {
            Contract.Ensures(Contract.Result<NetworkEntry<TNode, TConnection>>() != null);
            Contract.Ensures(Contract.Result<NetworkEntry<TNode, TConnection>>().NodeEntry != null);

            return new NetworkEntry<TNode, TConnection>(UpperConnectionEntries.ToArray(), LowerConnectionEntries.ToArray(), NodeEntry);
        }

        #endregion
    }
}
