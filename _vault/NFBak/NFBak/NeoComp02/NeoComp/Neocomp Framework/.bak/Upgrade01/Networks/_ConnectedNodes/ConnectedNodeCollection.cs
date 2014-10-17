using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Core;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace NeoComp.Networks
{
    public sealed class ConnectedNodeCollection<TConnection, TNode> : ReadOnlyCollection<ConnectedNode<TConnection, TNode>>
        where TConnection : IConnection
        where TNode : INode
    {
        internal ConnectedNodeCollection(ConnectedNode<TConnection, TNode>[] nodes)
            : base(nodes)
        {
            Contract.Requires(nodes != null);
        }

        #region Connections

        internal ConnectedNode<TConnection, TNode>[] ConnectedNodeArray
        {
            get { return (ConnectedNode<TConnection, TNode>[])base.Items; }
        } 

        #endregion

        #region View

        public ConnectedNodeCollection<TVConnection, TVNode> CreateView<TVConnection, TVNode>()
            where TVConnection : IConnection
            where TVNode : INode
        {
            return new ConnectedNodeCollection<TVConnection, TVNode>(
                (from cn in ConnectedNodeArray
                 where cn.Node is TVNode
                 select new ConnectedNode<TVConnection, TVNode>(
                     cn.UpperConnectionArray.OfType<TVConnection>(),
                     cn.LowerConnectionArray.OfType<TVConnection>(),
                     (TVNode)(object)cn.Node)).ToArray());

        } 

        #endregion
    }
}
