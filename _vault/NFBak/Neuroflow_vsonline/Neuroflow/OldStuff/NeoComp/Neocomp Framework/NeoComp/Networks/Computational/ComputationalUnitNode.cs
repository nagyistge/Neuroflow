using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Computations;

namespace NeoComp.Networks.Computational
{
    public abstract class ComputationalUnitNode<T> : ComputationalNode<T>
        where T : struct
    {
        protected internal override void Computation(ConnectionEntry<ComputationalConnection<T>>[] inputConnectionEntries, ConnectionEntry<ComputationalConnection<T>>[] outputConnectionEntries)
        {
            throw new NotImplementedException("TODO"); // TODO: protected internal override void Computation(ConnectionEntry<ComputationalConnection<T>>[] inputConnectionEntries, ConnectionEntry<ComputationalConnection<T>>[] outputConnectionEntries)
        }
    }
}
