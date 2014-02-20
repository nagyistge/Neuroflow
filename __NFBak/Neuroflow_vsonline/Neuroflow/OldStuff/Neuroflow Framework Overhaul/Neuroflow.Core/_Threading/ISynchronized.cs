using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Neuroflow.Core
{
    [ContractClass(typeof(ISynchronizedContract))]
    public interface ISynchronized
    {
        SyncContext SyncRoot { get; }
    }

    [ContractClassFor(typeof(ISynchronized))]
    abstract class ISynchronizedContract : ISynchronized
    {
        SyncContext ISynchronized.SyncRoot
        {
            get { return null; }
        }

        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            ISynchronized s = this;
            Contract.Invariant(s.SyncRoot != null);
        }
    }
}
