using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core.ComputationAPI;
using System.Diagnostics.Contracts;
using Neuroflow.Core;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Neuroflow.Networks
{
    public abstract class ComputationalNetwork<TNode, TConnection, TValue> : IComputationUnit<TValue>
        where TValue : struct
        where TNode : ComputationNode<TValue>
        where TConnection : ComputationConnection<TValue>
    {
        #region Constrcut

        protected ComputationalNetwork(int inputInterfaceLength, int outputInterfaceLength)
            : this(Guid.NewGuid(), inputInterfaceLength, outputInterfaceLength)
        {
            Contract.Requires(inputInterfaceLength >= 0);
            Contract.Requires(outputInterfaceLength >= 0);
        }

        protected ComputationalNetwork(Guid structuralUID, int inputInterfaceLength, int outputInterfaceLength)
        {
            Contract.Requires(inputInterfaceLength >= 0);
            Contract.Requires(outputInterfaceLength >= 0);

            ValueSpace = new ValueSpace<TValue>(structuralUID);
            SyncRoot = new SyncContext();
            InputInterface = new ComputationInterface<TValue>(SyncRoot, ValueSpace, inputInterfaceLength);
            OutputInterface = new ComputationInterface<TValue>(SyncRoot, ValueSpace, outputInterfaceLength);
        } 

        #endregion

        #region Props abd Field

        protected internal ValueSpace<TValue> ValueSpace { get; private set; }

        ComputationHandle computeHandle;

        IReset reset;

        public SyncContext SyncRoot { get; private set; }

        public ComputationInterface<TValue> InputInterface { get; private set; }

        public ComputationInterface<TValue> OutputInterface { get; private set; }

        public int InputInterfaceLength
        {
            get { return InputInterface.Length; }
        }

        public int OutputInterfaceLength
        {
            get { return OutputInterface.Length; }
        }

        protected int MaxNodeIndex { get; private set; }

        public bool IsRecurrent { get; private set; }

        public bool IsBuilt
        {
            get { lock (SyncRoot) return computeHandle != null; }
        }

        internal TNode[] nodes = new TNode[0];

        public ReadOnlyCollection<TNode> Nodes
        {
            get { return Array.AsReadOnly(nodes); }
        }

        internal TConnection[] connections = new TConnection[0];

        public ReadOnlyCollection<TConnection> Connections
        {
            get { return Array.AsReadOnly(connections); }
        }

        #endregion

        #region Query

        public IEnumerable<ComputationNetworkItem<double>> GetAllItems()
        {
            return nodes.Cast<ComputationNetworkItem<double>>().Concat(connections.Cast<ComputationNetworkItem<double>>());
        }

        #endregion

        #region Iteration

        public void Iteration()
        {
            lock (SyncRoot)
            {
                LockedIteration();
            }
        }

        public void LockedIteration()
        {
            if (computeHandle == null) throw GetHasNotBuiltEx();

            computeHandle.Run();
        } 

        #endregion

        #region Reset

        public void Reset()
        {
            lock (SyncRoot)
            {
                LockedReset();
            }
        }

        public void LockedReset()
        {
            if (reset == null) throw GetHasNotBuiltEx();

            reset.Reset();
        }

        #endregion

        #region Build

        public void Build(NetworkDefinition<TNode, TConnection> network)
        {
            Contract.Requires(network != null);
            Contract.Requires(network.ConnectionCount != 0);
            Contract.Requires(network.NodeCount != 0);

            lock (SyncRoot)
            {
                if (computeHandle == null)
                {
                    BuildInternal(network);
                }
                else
                {
                    throw new InvalidOperationException("Computation network has already built.");
                }
            }
        }

        void BuildInternal(NetworkDefinition<TNode, TConnection> network)
        {
            if (network.ConnectionCount == 0 || network.NodeCount == 0) throw new InvalidOperationException("Network is empty.");

            var nodes = new LinkedList<TNode>();
            int beginOfOutputIndex = network.MaxNodeIndex - OutputInterfaceLength + 1;

            var initializedConns = new Dictionary<ConnectionIndex, ConnectionEntry<TConnection>>();

            // Lower Connections:

            foreach (var nodeEntry in network.NodeEntries)
            {
                var lowers = network.GetLowerConnections(nodeEntry.Index);

                int? inputValueIndex = null;

                if (nodeEntry.Node.IsOpeartionNode)
                {
                    foreach (var lc in lowers)
                    {
                        // Connected to output?
                        if (lc.Index.LowerNodeIndex >= beginOfOutputIndex)
                        {
                            if (nodeEntry.Node.IsOpeartionNode && inputValueIndex != null)
                            {
                                throw new InvalidOperationException("There can be only one connection to network output nodes.");
                            }

                            inputValueIndex = OutputInterface.GetReferenceIndex(lc.Index.LowerNodeIndex - beginOfOutputIndex);
                            break;
                        }
                    }

                    if (inputValueIndex == null)
                    {
                        inputValueIndex = ValueSpace.Declare();
                    }
                }

                //Process Lowers:
                foreach (var lc in lowers)
                {
                    if (initializedConns.ContainsKey(lc.Index)) continue;

                    // Conneted to input?
                    if (lc.Index.LowerNodeIndex < InputInterfaceLength)
                    {
                        continue;
                    }

                    if (inputValueIndex == null)
                    {
                        Debug.Assert(!nodeEntry.Node.IsOpeartionNode);
                        
                        // Connected to output?
                        if (lc.Index.LowerNodeIndex >= beginOfOutputIndex)
                        {
                            inputValueIndex = OutputInterface.GetReferenceIndex(lc.Index.LowerNodeIndex - beginOfOutputIndex);
                        }
                    }

                    if (lc.Index.LowerNodeIndex <= lc.Index.UpperNodeIndex) IsRecurrent = true;

                    InitializeConnection(ValueSpace, lc, inputValueIndex);

                    initializedConns.Add(lc.Index, lc);
                }
            }

            // Upper Connections:

            foreach (var nodeEntry in network.NodeEntries)
            {
                var uppers = network.GetUpperConnections(nodeEntry.Index);

                // Process Uppers:
                foreach (var uc in uppers)
                {
                    int? inputValueIndex = null;
                    
                    if (initializedConns.ContainsKey(uc.Index)) continue;
                    
                    // Connected to output?
                    if (uc.Index.UpperNodeIndex >= beginOfOutputIndex)
                    {
                        continue;
                    }

                    // Conneted to input?
                    if (uc.Index.UpperNodeIndex < InputInterfaceLength)
                    {
                        inputValueIndex = InputInterface.GetReferenceIndex(uc.Index.UpperNodeIndex);
                    }

                    if (uc.Index.LowerNodeIndex <= uc.Index.UpperNodeIndex) IsRecurrent = true;

                    InitializeConnection(ValueSpace, uc, inputValueIndex);

                    initializedConns.Add(uc.Index, uc);
                }
            }

            // Nodes:            
            
            foreach (var nodeEntry in network.NodeEntries)
            {
                var uppers = network.GetUpperConnections(nodeEntry.Index); 
                var lowers = network.GetLowerConnections(nodeEntry.Index); 
                var initializedUppers = new LinkedList<ConnectionEntry<TConnection>>();
                var initializedLowers = new LinkedList<ConnectionEntry<TConnection>>();

                ConnectionEntry<TConnection> entry;
                
                foreach (var uc in uppers)
                {
                    if (initializedConns.TryGetValue(uc.Index, out entry))
                    {
                        initializedUppers.AddLast(entry);
                    }
                }

                foreach (var lc in lowers)
                {
                    if (initializedConns.TryGetValue(lc.Index, out entry))
                    {
                        initializedLowers.AddLast(entry);
                    }
                }
                
                if ((initializedUppers.Count == 0 && !nodeEntry.Node.SupportsNoUpperConnections) ||
                    (initializedLowers.Count == 0 && !nodeEntry.Node.SupportsNoLowerConnections))
                {
                    continue;
                }

                nodeEntry.Node.Initialize(nodeEntry.Index, ValueSpace, initializedUppers.Select(e => e.Connection), initializedLowers.Select(e => e.Connection));

                nodes.AddLast(nodeEntry.Node);
            }

            Contract.Assert(computeHandle == null);

            this.nodes = nodes.ToArray();
            this.connections = initializedConns.Values.Select(c => c.Connection).ToArray();

            MaxNodeIndex = network.MaxNodeIndex;

            ComputeNodeTimeFrames();

            NodesCreated();

            Parallel.Invoke(
                () => computeHandle = CompileComputeHandle(),
                () => reset = CreateReset());

            Built();
        }

        private static void InitializeConnection(ValueSpace<TValue> valueSpace, ConnectionEntry<TConnection> conn, int? inputValueIndex)
        {
            if (conn.Connection.IsInitialized) throw new InvalidOperationException("Connection " + conn.Index + " is already initialized.");

            conn.Connection.InitializeWithInputIndex(conn.Index, valueSpace, inputValueIndex);
        }

        private void ComputeNodeTimeFrames()
        {
            int timeFrameIgnore = InputInterfaceLength - 1;
            int timeFrame = 0;

            foreach (var node in nodes)
            {
                if (node.UpperConnections.Any(c => c.Index.UpperNodeIndex >= timeFrameIgnore))
                {
                    timeFrame++;
                    timeFrameIgnore = node.Index;
                }

                node.TimeFrame = timeFrame;
            }
        }

        private ComputationHandle CompileComputeHandle()
        {
            var builder = new ComputationBuilder<TValue>();

            foreach (var node in nodes)
            {
                var block = node.GetComputationBlock();

                block.TimeFrame = node.TimeFrame;

                builder.AddBlock(block);
            }

            return builder.Compile(ValueSpace, "Compute");
        }

        protected abstract IReset CreateReset();        

        protected virtual void NodesCreated() { }

        protected virtual void Built() { }

        #endregion

        #region Cleanup

        public void Dispose()
        {
            ValueSpace.Dispose();
        } 

        #endregion

        #region Error

        internal static Exception GetHasNotBuiltEx()
        {
            return new InvalidOperationException("Computation network has not built.");
        }

        #endregion
    }
}
