using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Networks.Computational.Neural
{
    internal sealed class BackwardPropagatorHelper
    {
        internal BackwardPropagatorHelper(IBackwardPropagator propagator, ConnectionEntry<ComputationalConnection<double>>[] inputs, ConnectionEntry<ComputationalConnection<double>>[] outputs)
        {
            Contract.Requires(propagator != null);
            Contract.Requires(inputs != null);
            Contract.Requires(outputs != null);

            Propagator = propagator;
            InputEntries = (from e in inputs
                            let bc = e.Connection as IBackwardConnection
                            where bc != null
                            select new BackwardConnectionEntry(e.Index, bc)).ToArray();
            OutputEntries = (from e in outputs
                             let bc = e.Connection as IBackwardConnection
                             where bc != null
                             select new BackwardConnectionEntry(e.Index, bc)).ToArray();
        }

        internal IBackwardPropagator Propagator { get; private set; }

        internal BackwardConnectionEntry[] InputEntries { get; private set; }

        internal BackwardConnectionEntry[] OutputEntries { get; private set; }

        internal void Propagate(bool isNewBatch)
        {
            Propagator.BackPropagate(OutputEntries, InputEntries, isNewBatch);
        }
    }
}
