using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp;
using System.Diagnostics.Contracts;
using NeoComp.Computations;
using System.Runtime.Serialization;
using NeoComp.Threading;

namespace NeoComp.ComputationalNetworks
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = xmlns.NeoCompNS, Name = "compNetIntf")]
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
