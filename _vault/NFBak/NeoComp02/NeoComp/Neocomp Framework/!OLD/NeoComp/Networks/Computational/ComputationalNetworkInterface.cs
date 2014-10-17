using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Diagnostics.Contracts;
using NeoComp.Computations;

namespace NeoComp.Networks.Computational
{
    [Serializable]
    public sealed class ComputationalNetworkInterface<T> : ComputationalInterface<T>, INetworkInterface<T>
    {
        internal ComputationalNetworkInterface(int length, SyncContext syncRoot)
            : base(length, syncRoot)
        {
            Contract.Requires(length > 0);
            Contract.Requires(syncRoot != null);
        }

        ComputationalValue<T> INetworkInterface<T>.GetComputationalValue(int index)
        {
            return Values[index];
        }

        void INetworkInterface<T>.SetValueAt(int index, T value)
        {
            Values[index].Value = value;
        }
    }
}
