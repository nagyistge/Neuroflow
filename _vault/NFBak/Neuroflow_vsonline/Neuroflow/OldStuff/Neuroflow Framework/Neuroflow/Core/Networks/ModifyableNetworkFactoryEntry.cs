using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Collections.ObjectModel;
using Neuroflow.Core;

namespace Neuroflow.Core.Networks
{
    public sealed class ModifyableNetworkFactoryEntry<TNode, TConnection>
    {
        #region Contruct and Create

        internal ModifyableNetworkFactoryEntry(NetworkFactoryEntry<TNode, TConnection> networkFactoryEntry)
        {
            Contract.Requires(networkFactoryEntry != null);
            Contract.Requires(networkFactoryEntry.NodeFactoryEntry != null);

            NodeFactoryEntry = networkFactoryEntry.NodeFactoryEntry;
            UpperConnectionFactoryEntries = new NoAddCollection<ConnectionFactoryEntry<TConnection>>(networkFactoryEntry.UpperConnectionFactoryEntries.ToList());
            LowerConnectionFactoryEntries = new NoAddCollection<ConnectionFactoryEntry<TConnection>>(networkFactoryEntry.LowerConnectionFactoryEntries.ToList());
        }

        #endregion

        #region Properties

        public NodeFactoryEntry<TNode> NodeFactoryEntry { get; private set; }

        public IFactory<TNode> NodeFactory
        {
            get { return NodeFactoryEntry.NodeFactory; }
            set
            {
                Contract.Requires(value != null);
                NodeFactoryEntry.NodeFactory = value;
            }
        }

        public IList<ConnectionFactoryEntry<TConnection>> UpperConnectionFactoryEntries { get; private set; }

        public IList<ConnectionFactoryEntry<TConnection>> LowerConnectionFactoryEntries { get; private set; }

        #endregion

        #region CreateNE

        internal ModifyableNetworkEntry<TNode, TConnection> CreateModifyableNetworkEntry()
        {
            Contract.Ensures(Contract.Result<ModifyableNetworkEntry<TNode, TConnection>>() != null);
            Contract.Ensures(Contract.Result<ModifyableNetworkEntry<TNode, TConnection>>().Node != null);

            return new ModifyableNetworkEntry<TNode, TConnection>(this);
        }

        #endregion
    }
}
