using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core.Computations;
using Neuroflow.Core.Networks;

namespace Neuroflow.Core.ComputationalNetworks
{
    public abstract class ComputationalUnitNode<T> : ComputationNode<T>
        where T : struct
    {
        protected internal override void Computation(ConnectionEntry<ComputationConnection<T>>[] inputConnectionEntries, ConnectionEntry<ComputationConnection<T>>[] outputConnectionEntries)
        {
            throw new NotImplementedException("TODO"); // TODO: protected internal override void Computation(ConnectionEntry<ComputationalConnection<T>>[] inputConnectionEntries, ConnectionEntry<ComputationalConnection<T>>[] outputConnectionEntries)
        }
    }
}
