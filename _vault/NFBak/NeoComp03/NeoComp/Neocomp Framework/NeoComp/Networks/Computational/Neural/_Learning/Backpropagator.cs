﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Core;

namespace NeoComp.Networks.Computational.Neural
{
    public abstract class Backpropagator
    {
        protected Backpropagator(IBackwardNode node)
        {
            Contract.Requires(node != null);
            Node = node;
        }

        public IBackwardNode Node { get; private set; }

        protected ReadOnlyArray<BackwardConnectionEntry> InputConnectionEntries { get; private set; }

        protected ReadOnlyArray<BackwardConnectionEntry> OutputConnectionEntries { get; private set; }

        internal void Initialize(ConnectionEntry<ComputationalConnection<double>>[] inputs, ConnectionEntry<ComputationalConnection<double>>[] outputs)
        {
            InputConnectionEntries = ReadOnlyArray.Wrap((from e in inputs
                                                         let bc = e.Connection as IBackwardConnection
                                                         where bc != null
                                                         select new BackwardConnectionEntry(e.Index, bc)).ToArray());

            OutputConnectionEntries = ReadOnlyArray.Wrap((from e in outputs
                                                          let bc = e.Connection as IBackwardConnection
                                                          where bc != null
                                                          select new BackwardConnectionEntry(e.Index, bc)).ToArray());

            Initialized();
        }

        protected virtual void Initialized() { }

        protected internal abstract void TrackForwardInformation();

        protected internal abstract void Backpropagate();
    }
}
