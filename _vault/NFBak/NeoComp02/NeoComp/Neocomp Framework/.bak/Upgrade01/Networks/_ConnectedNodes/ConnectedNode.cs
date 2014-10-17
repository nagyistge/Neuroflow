using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace NeoComp.Networks
{
    public sealed class ConnectedNode<TConnection, TNode>  : IConnectedNode
        where TConnection : IConnection
        where TNode : INode
    {
        internal ConnectedNode(IEnumerable<TConnection> upperConnections, IEnumerable<TConnection> lowerConnections, TNode node)
        {
            Contract.Requires(upperConnections != null);
            Contract.Requires(lowerConnections != null);
            
            UpperConnectionArray = upperConnections.ToArray();
            LowerConnectionArray = lowerConnections.ToArray();
            Node = node;
        }
        
        internal TConnection[] UpperConnectionArray { get; private set; }

        internal TConnection[] LowerConnectionArray { get; private set; }

        public ReadOnlyCollection<TConnection> UpperConnections
        {
            get { return new ReadOnlyCollection<TConnection>(((IList<TConnection>)UpperConnectionArray)); }
        }

        public ReadOnlyCollection<TConnection> LowerConnections
        {
            get { return new ReadOnlyCollection<TConnection>(((IList<TConnection>)LowerConnectionArray)); }
        }

        public TNode Node { get; private set; }

        #region IConnectedNode Members

        IEnumerable<IConnection> IConnectedNode.UpperConnections
        {
            get { return UpperConnectionArray.Cast<IConnection>(); }
        }

        IEnumerable<IConnection> IConnectedNode.LowerConnections
        {
            get { return LowerConnectionArray.Cast<IConnection>(); }
        }

        INode IConnectedNode.Node
        {
            get { return Node; }
        }

        #endregion
    }
}
