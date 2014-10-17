using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core;
using System.Diagnostics.Contracts;
using Neuroflow.Core.Computations;
using System.Runtime.Serialization;
using Neuroflow.Core.Threading;

namespace Neuroflow.Core.ComputationalNetworks
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = xmlns.Neuroflow, Name = "compNetIntf")]
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
