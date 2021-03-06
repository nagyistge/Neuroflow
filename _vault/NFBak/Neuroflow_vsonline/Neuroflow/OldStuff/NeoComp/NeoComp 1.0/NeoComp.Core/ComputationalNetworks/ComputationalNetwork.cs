﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Computations;
using NeoComp;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Diagnostics;
using System.Runtime.Serialization;
using NeoComp.Networks;
using NeoComp.Threading;

namespace NeoComp.ComputationalNetworks
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = xmlns.NeoCompNS)]
    public abstract class ComputationalNetwork<T> : 
        Network<ComputationNode<T>, ComputationConnection<T>>,
        IComputationUnit<T>, 
        ISynchronized,
        IInterfaced,
        IResetable
        where T : struct
    {
        #region Constructor

        protected ComputationalNetwork(ComputationalNetworkFactory<T> factory)
            : base(factory)
        {
            Contract.Requires(factory != null);

            SyncRoot = new SyncContext();

            InitializeInterfaces(factory.InputInterfaceLength, factory.OutputInterfaceLength);
            InitializeNodes();
        }

        #endregion

        #region Properties And Fields

        [DataMember(Name = "uid")]
        Guid uid = Guid.NewGuid();

        public Guid UID
        {
            get { return uid; }
        }

        [DataMember(Name = "iIntf")]
        ComputationalNetworkInterface<T> inputInterface;

        public IInputInterface<T> InputInterface
        {
            get { return inputInterface; }
        }

        [DataMember(Name = "oIntf")]
        ComputationalNetworkInterface<T> outputInterface;

        public IOutputInterface<T> OutputInterface
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

        private ComputationalNetworkInterface<T> CreateInterface(int length)
        {
            Contract.Requires(length > 0);

            return new ComputationalNetworkInterface<T>(length, SyncRoot); 
        }

        private void AdaptValues(ComputationalNetworkInterface<T> intf, IEnumerable<ConnectionEntry<ComputationConnection<T>>> entries, int beginIndex, bool upper)
        {
            foreach (var entry in entries)
            {
                int iidx = (upper ? entry.Index.UpperNodeIndex : entry.Index.LowerNodeIndex) - beginIndex;
                entry.Connection.AdaptValue(intf.GetComputationalValue(iidx));
            }
        }

        private void InitializeNodes()
        {
            foreach (var e in EntryArray)
            {
                e.NodeEntry.Node.Initialize(e.UpperConnectionEntryArray, e.LowerConnectionEntryArray);
            }
        }

        #endregion

        #region Entry Access

        public IEnumerable<ConnectionEntry<ComputationConnection<T>>> GetInputConnectionEntries()
        {
            int endIdx = inputInterface.Length;
            return from entry in EntryArray
                   from uc in entry.UpperConnectionEntryArray
                   where uc.Index.UpperNodeIndex < endIdx
                   select uc;
        }

        public IEnumerable<ConnectionEntry<ComputationConnection<T>>> GetOutputConnectionEntries()
        {
            int endIdx = MaxEntryIndex - outputInterface.Length;
            return from entry in EntryArray
                   from lc in entry.LowerConnectionEntryArray
                   where lc.Index.LowerNodeIndex > endIdx
                   select lc;
        } 

        #endregion

        #region Output Generation

        public void Iteration()
        {
            lock (SyncRoot)
            {
                foreach (var entry in EntryArray)
                {
                    entry.NodeEntry.Node.Computation(entry.UpperConnectionEntryArray, entry.LowerConnectionEntryArray);
                }
            }
        }

        #endregion

        #region Item Access

        public override NetworkItems<ComputationNode<T>, ComputationConnection<T>> GetItems()
        {
            var nodeEntries = EntryArray.Select(e => e.NodeEntry);
            var connEntries = from e in EntryArray
                              from uce in e.UpperConnectionEntryArray
                              select uce;
            connEntries = connEntries.Concat(GetOutputConnectionEntries());
            return new NetworkItems<ComputationNode<T>, ComputationConnection<T>>(nodeEntries, connEntries);
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
            var clone = (ComputationalNetwork<T>)base.Clone();
            clone.uid = Guid.NewGuid();
            return clone;
        }

        #endregion

        #region Reset Support

        public IReset GetReset()
        {
            return new ComputationalNetworkReset<T>(this);
        }

        protected internal virtual void Reset() { }

        #endregion
    }
}
