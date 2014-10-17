using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using NeoComp.Computations;
using NeoComp.Networks;

namespace NeoComp.ComputationalNetworks
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = xmlns.NeoCompNS)]
    public abstract class ComputationNode<T> : IReset
        where T : struct
    {
        protected internal virtual void Initialize(ConnectionEntry<ComputationConnection<T>>[] inputConnectionEntries, ConnectionEntry<ComputationConnection<T>>[] outputConnectionEntries)
        {
        }
        
        protected internal abstract void Computation(ConnectionEntry<ComputationConnection<T>>[] inputConnectionEntries, ConnectionEntry<ComputationConnection<T>>[] outputConnectionEntries);

        protected abstract void Reset();

        #region IReset Members

        void IReset.Reset()
        {
            Reset();
        }

        #endregion
    }
}
