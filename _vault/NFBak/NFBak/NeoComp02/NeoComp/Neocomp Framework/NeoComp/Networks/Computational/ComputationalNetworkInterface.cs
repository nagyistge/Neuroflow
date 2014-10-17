using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Diagnostics.Contracts;
using NeoComp.Computations;
using System.Runtime.Serialization;

namespace NeoComp.Networks.Computational
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = NeoComp.xmlns, Name = "compNetIntf")]
    public sealed class ComputationalNetworkInterface<T> : ComputationInterface<T>
        where T : struct
    {
        internal ComputationalNetworkInterface(int length, SyncContext syncRoot)
            : base(length, syncRoot)
        {
            Contract.Requires(length > 0);
            Contract.Requires(syncRoot != null);
        }

        internal ComputationValue<T> GetComputationalValue(int index)
        {
            return Values[index];
        }
    }
}
