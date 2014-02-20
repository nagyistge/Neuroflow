using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using NeoComp.Core;
using System.Threading.Tasks;
using System.Threading;
using NeoComp.Computations;

namespace NeoComp.Networks
{
    public interface IComputationalNetwork : INetwork
    {
        IComputationalInterface InputInterface { get; }

        IComputationalInterface OutputInterface { get; }

        void GenerateOutput();

        void GenerateOutput(CancellationToken cancellationToken);

        void AddNode(int nodeIndex, IComputationalNetwork logicalSubnetwork);

        void TryAddNode(int nodeIndex, IComputationalNetwork logicalSubnetwork);

        bool RemoveNode(IComputationalNetwork logicalSubnetwork);
    }
    
    public abstract class ComputationalNetwork<TConnection, T> : Network<TConnection, ComputationalNode<TConnection, T>>, IComputationalUnit<T>, IComputationalNetwork
        where TConnection : ComputationalConnection<T>
    {
        #region Constructor

        public ComputationalNetwork(int inputInterfaceLength, int outputInterfaceLength)
        {
            if (inputInterfaceLength < 0) throw new ArgumentOutOfRangeException("inputInterfaceLength");
            if (outputInterfaceLength < 0) throw new ArgumentOutOfRangeException("outputInterfaceLength");
            InputInterface = new NetworkInterface<TConnection, T>(this, inputInterfaceLength);
            OutputInterface = new NetworkInterface<TConnection, T>(this, outputInterfaceLength);
        } 

        #endregion

        #region Properties

        public NetworkInterface<TConnection, T> InputInterface { get; private set; }

        public NetworkInterface<TConnection, T> OutputInterface { get; private set; } 

        #endregion

        #region Subnetwork Management

        public virtual void AddNode(int nodeIndex, ComputationalNetwork<TConnection, T> subnetwork)
        {
            Args.IsNotNull(subnetwork, "logicalSubnetwork");

            AddNode(nodeIndex, new ComputationalSubnetworkNode<TConnection, T>(subnetwork));
        }

        public virtual bool TryAddNode(int nodeIndex, ComputationalNetwork<TConnection, T> subnetwork)
        {
            Args.IsNotNull(subnetwork, "logicalSubnetwork");

            return TryAddNode(nodeIndex, new ComputationalSubnetworkNode<TConnection, T>(subnetwork));
        }

        public virtual bool RemoveNode(ComputationalNetwork<TConnection, T> subnetwork)
        {
            Args.IsNotNull(subnetwork, "logicalSubnetwork");

            lock (SyncRoot)
            {
                var ops = GetNodes().OfType<ComputationalSubnetworkNode<TConnection, T>>().Where(op => op.InnerNetwork == subnetwork);
                int removedCount = 0;
                foreach (var op in ops.ToList())
                {
                    if (RemoveNode(op)) removedCount++;
                }
                return removedCount != 0;
            }
        }

        #endregion

        #region Output Generation

        public void GenerateOutput()
        {
            GenerateOutput(null);
        }

        public void GenerateOutput(CancellationToken cancellationToken)
        {
            GenerateOutput(cancellationToken);
        }

        private void GenerateOutput(CancellationToken? cancellationToken)
        {
            lock (SyncRoot)
            {
                foreach (var connectedNode in ConnectedNodes.ConnectedNodeArray)
                {
                    if (cancellationToken.HasValue && cancellationToken.Value.IsCancellationRequested) break;
                    connectedNode.Node.ComputeOutputValue(
                        connectedNode.UpperConnectionArray,
                        connectedNode.LowerConnectionArray);
                }
            }
        }

        #endregion

        #region Node Generation

        protected override bool IsValid(ComputationalNode<TConnection, T> node)
        {
            return node.Index >= InputInterface.Length && node.Index <= Size - OutputInterface.Length;
        }

        protected override bool IsValid(ConnectedNode<TConnection, ComputationalNode<TConnection, T>> connectedNode)
        {
            return connectedNode.LowerConnectionArray.Length > 0 && connectedNode.UpperConnectionArray.Length > 0; // Has Output And Input
        }

        protected override bool IsValid(TConnection connection)
        {
            return connection.Index.LowerNodeIndex >= InputInterface.Length &&
                connection.Index.UpperNodeIndex <= Size - OutputInterface.Length;
        }

        protected override ConnectedNodeCollection<TConnection, ComputationalNode<TConnection, T>> GenerateConnectedNodes()
        {
            InitializeConnections();
            return base.GenerateConnectedNodes();
        }

        private void InitializeConnections()
        {
            int outStartIndex = (Size + 1) - OutputInterface.Length;
            foreach (var conn in GetConnections())
            {
                int upperNodeIndex = conn.Index.UpperNodeIndex;
                int lowerNodeIndex = conn.Index.LowerNodeIndex;
                if (upperNodeIndex < InputInterface.Length)
                {
                    var netv = InputInterface.GetDataValue(upperNodeIndex);
                    conn.Initialize(netv);
                    InputInterface.Connections.GetCollection(upperNodeIndex).Add(conn);
                }
                else if (lowerNodeIndex >= outStartIndex)
                {
                    int outValueIndex = lowerNodeIndex - outStartIndex;
                    Debug.Assert(outValueIndex >= 0 && outValueIndex < OutputInterface.Length);
                    var netv = OutputInterface.GetDataValue(outValueIndex);
                    conn.Initialize(netv);
                    OutputInterface.Connections.GetCollection(outValueIndex).Add(conn);
                }
                else
                {
                    conn.Initialize(new ComputationalValue<T>());
                }
            }
        }

        protected override void ReleaseContextItems()
        {
            base.ReleaseContextItems();
            InputInterface.Connections.Clear();
            OutputInterface.Connections.Clear();
        }

        #endregion

        #region IO Connection Infos

        private IEnumerable<TC> GetInputConnectionsEnum<TC>()
            where TC : IConnection
        {
            var cna = ConnectedNodes.ConnectedNodeArray;
            int endIdx = InputInterface.Length;
            return from cn in cna
                   from uc in cn.UpperConnectionArray.OfType<TC>()
                   where uc.Index.UpperNodeIndex < endIdx
                   select uc;
        }

        private IEnumerable<TC> GetOutputConnectionsEnum<TC>()
            where TC : IConnection
        {
            var cna = ConnectedNodes.ConnectedNodeArray;
            int endIdx = Size - OutputInterface.Length;
            return from cn in cna
                   from lc in cn.LowerConnectionArray.OfType<TC>()
                   where lc.Index.LowerNodeIndex > endIdx
                   select lc;
        }

        public ConnectionInfo<TConnection> GetInputConnectionInfo()
        {
            var info = new ConnectionInfo<TConnection>(InputInterface.Length);
            foreach (var conn in GetInputConnectionsEnum<TConnection>())
            {
                int idx = conn.Index.UpperNodeIndex;
                Debug.Assert(idx < info.Count);
                info[idx].AddInternal(conn);
            }
            return info;
        }

        public ConnectionInfo<TConnection> GetOutputConnectionInfo()
        {
            var info = new ConnectionInfo<TConnection>(OutputInterface.Length);
            int sub = Size - OutputInterface.Length + 1;
            foreach (var conn in GetOutputConnectionsEnum<TConnection>())
            {
                int idx = conn.Index.LowerNodeIndex - sub;
                Debug.Assert(idx < info.Count);
                info[idx].AddInternal(conn);
            }
            return info;
        }

        public ConnectionInfo<TC> GetInputConnectionInfo<TC>()
            where TC : IConnection
        {
            var info = new ConnectionInfo<TC>(InputInterface.Length);
            foreach (var conn in GetInputConnectionsEnum<TC>())
            {
                int idx = conn.Index.UpperNodeIndex;
                Debug.Assert(idx < info.Count);
                info[idx].AddInternal(conn);
            }
            return info;
        }

        public ConnectionInfo<TC> GetOutputConnectionInfo<TC>()
            where TC : IConnection
        {
            var info = new ConnectionInfo<TC>(OutputInterface.Length);
            int sub = Size - OutputInterface.Length + 1;
            foreach (var conn in GetOutputConnectionsEnum<TC>())
            {
                int idx = conn.Index.LowerNodeIndex - sub;
                Debug.Assert(idx < info.Count);
                info[idx].AddInternal(conn);
            }
            return info;
        }

        #endregion

        #region IComputationalNetwork Members

        IComputationalInterface IComputationalNetwork.InputInterface
        {
            get { return this.InputInterface; }
        }

        IComputationalInterface IComputationalNetwork.OutputInterface
        {
            get { return this.OutputInterface; }
        }

        void IComputationalNetwork.AddNode(int nodeIndex, IComputationalNetwork subnetwork)
        {
            AddNode(nodeIndex, Args.CastAs<ComputationalNetwork<TConnection, T>>(subnetwork, "subnetwork"));
        }

        void IComputationalNetwork.TryAddNode(int nodeIndex, IComputationalNetwork subnetwork)
        {
            TryAddNode(nodeIndex, Args.CastAs<ComputationalNetwork<TConnection, T>>(subnetwork, "subnetwork"));
        }

        bool IComputationalNetwork.RemoveNode(IComputationalNetwork subnetwork)
        {
            return RemoveNode(Args.CastAs<ComputationalNetwork<TConnection, T>>(subnetwork, "subnetwork"));
        }

        #endregion

        #region IComputationalUnit<T> Members

        void IComputationalUnit<T>.ComputeOutput(CancellationToken? cancellationToken)
        {
            if (cancellationToken.HasValue)
            {
                GenerateOutput(cancellationToken.Value);
            }
            else
            {
                GenerateOutput();
            }
        }

        IComputationalInterface<T> IComputationalUnit<T>.InputInterface
        {
            get { return InputInterface; }
        }

        IComputationalInterface<T> IComputationalUnit<T>.OutputInterface
        {
            get { return OutputInterface; }
        } 

        #endregion
    }
}
