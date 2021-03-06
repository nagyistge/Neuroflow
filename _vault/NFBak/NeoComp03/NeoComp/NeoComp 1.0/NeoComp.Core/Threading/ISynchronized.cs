﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Threading
{
    [ContractClass(typeof(ISynchronizedContract))]
    public interface ISynchronized
    {
        SyncContext SyncRoot { get; }
    }

    [ContractClassFor(typeof(ISynchronized))]
    sealed class ISynchronizedContract : ISynchronized
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
