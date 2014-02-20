using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using NeoComp.Computations;

namespace NeoComp.Networks.Computational
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = NeoComp.xmlns)]
    public abstract class ComputationalNode<T> : IReset
        where T : struct
    {
        protected internal virtual void Initialize(ConnectionEntry<ComputationalConnection<T>>[] inputConnectionEntries, ConnectionEntry<ComputationalConnection<T>>[] outputConnectionEntries)
        {
        }
        
        protected internal abstract void Computation(ConnectionEntry<ComputationalConnection<T>>[] inputConnectionEntries, ConnectionEntry<ComputationalConnection<T>>[] outputConnectionEntries);

        protected abstract void Reset();

        #region IReset Members

        void IReset.Reset()
        {
            Reset();
        }

        #endregion
    }
}
