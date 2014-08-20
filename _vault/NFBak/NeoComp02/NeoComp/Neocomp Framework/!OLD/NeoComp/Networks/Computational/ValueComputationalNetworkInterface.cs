using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Computations;
using System.Diagnostics.Contracts;
using NeoComp.Core;
using System.Runtime.Serialization;

namespace NeoComp.Networks.Computational
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = NeoComp.xmlns, Name = "valIntf")]
    public sealed class ValueComputationalNetworkInterface<T> : ValueInterface<T>, INetworkInterface<T>
        where T : struct
    {
        internal ValueComputationalNetworkInterface(int length, SyncContext syncRoot)
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
