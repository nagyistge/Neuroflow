using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Computations;
using NeoComp.Core;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace NeoComp.Networks.Computational
{
    public static class TEMP
    {
        internal static SpinLock sl = new SpinLock();

        public static int msTime = 0;
    }
    
    [Serializable]
    [DataContract(IsReference = true, Namespace = NeoComp.xmlns)]
    public abstract class ComputationalNetwork<T> : 
        Network<ComputationalNode<T>, ComputationalConnection<T>>,
        IComputationalUnit<T>, 
        ISynchronized,
        IInterfaced
    {
        #region Constructor

        protected ComputationalNetwork(ComputationalNetworkFactory<T> factory)
            : base(factory, factory)
        {
            Contract.Requires(factory != null);

            SyncRoot = new SyncContext();

            InitializeInterfaces(factory.InputInterfaceLength, factory.OutputInterfaceLength);
            InitializeOperationNodes();
        }

        #endregion

        #region Properties And Fields

        [DataMember(Name = "iIntf")]
        INetworkInterface<T> inputInterface;

        public IComputationalInterface<T> InputInterface
        {
            get { return inputInterface; }
        }

        [DataMember(Name = "oIntf")]
        INetworkInterface<T> outputInterface;

        public IComputationalInterface<T> OutputInterface
        {
            get { return outputInterface; }
        }

        [DataMember(Name = "syncRoot")]
        public SyncContext SyncRoot { get; private set; }

        #endregion

        #region Initialize

        private void InitializeInterfaces(int inputInterfaceLength, int outputInterfaceLength)
        {
            inputInterface = CreateInterface(inputInterfaceLength);
            outputInterface = CreateInterface(outputInterfaceLength);

            AdaptValues(inputInterface, GetInputConnectionEntries(), 0, true);
            AdaptValues(outputInterface, GetOutputConnectionEntries(), MaxEntryIndex - outputInterface.Length + 1, false);
        }

        internal virtual INetworkInterface<T> CreateInterface(int length)
        {
            Contract.Requires(length > 0);

            return new ComputationalNetworkInterface<T>(length, SyncRoot); 
        }

        private void AdaptValues(INetworkInterface<T> intf, IEnumerable<ConnectionEntry<ComputationalConnection<T>>> entries, int beginIndex, bool upper)
        {
            foreach (var entry in entries)
            {
                int iidx = (upper ? entry.Index.UpperNodeIndex : entry.Index.LowerNodeIndex) - beginIndex;
                entry.Connection.AdaptValue(intf.GetComputationalValue(iidx));
            }
        }

        private void InitializeOperationNodes()
        {
            foreach (var e in EntryArray)
            {
                var on = e.NodeEntry.Node as OperationNode<T>;
                if (on != null) on.InitializeOperationContext(e.UpperConnectionEntryArray, e.LowerConnectionEntryArray);
            }
        }

        #endregion

        #region Entry Access

        public IEnumerable<ConnectionEntry<ComputationalConnection<T>>> GetInputConnectionEntries()
        {
            int endIdx = inputInterface.Length;
            return from entry in EntryArray
                   from uc in entry.UpperConnectionEntryArray
                   where uc.Index.UpperNodeIndex < endIdx
                   select uc;
        }

        public IEnumerable<ConnectionEntry<ComputationalConnection<T>>> GetOutputConnectionEntries()
        {
            int endIdx = MaxEntryIndex - outputInterface.Length;
            return from entry in EntryArray
                   from lc in entry.LowerConnectionEntryArray
                   where lc.Index.LowerNodeIndex > endIdx
                   select lc;
        } 

        #endregion

        #region Output Generation

        public void ComputeOutput(CancellationToken? cancellationToken = null)
        {
            lock (SyncRoot)
            {
                var sw = new Stopwatch();
                sw.Start();
                foreach (var entry in EntryArray)
                {
                    if (cancellationToken.IsCancellationRequested()) break;
                    entry.NodeEntry.Node.Computation(entry.UpperConnectionEntryArray, entry.LowerConnectionEntryArray);
                }
                sw.Stop();
                bool taken = false;
                try
                {
                    TEMP.sl.Enter(ref taken);
                    TEMP.msTime += (int)sw.ElapsedMilliseconds;
                }
                finally
                {
                    if (taken) TEMP.sl.Exit();
                }
            }
        }

        #endregion

        #region Comp. Unit Impl.

        void IComputationalUnit<T>.ComputeOutput(CancellationToken? cancellationToken)
        {
            ComputeOutput(cancellationToken);
        }

        #endregion

        #region IInterfaced Impl.

        int IInterfaced.InputInterfaceLength
        {
            get { return inputInterface.Length; }
        }

        int IInterfaced.OutputInterfaceLength
        {
            get { return outputInterface.Length; }
        } 

        #endregion

        #region Clone

        new public ComputationalNetwork<T> Clone()
        {
            return (ComputationalNetwork<T>)base.Clone();
        }

        #endregion
    }
}
