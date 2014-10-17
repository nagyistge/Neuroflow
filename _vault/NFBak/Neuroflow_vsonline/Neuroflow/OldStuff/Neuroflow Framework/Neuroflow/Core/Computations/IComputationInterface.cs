using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Neuroflow.Core.Computations
{
    [ContractClass(typeof(IComputationInterfaceContract<>))]
    public interface IComputationInterface<T>
        where T : struct
    {
        int Length { get; }

        T this[int index] { get; set; }
    }

    [ContractClassFor(typeof(IComputationInterface<>))]
    class IComputationInterfaceContract<T> : IComputationInterface<T>
        where T : struct
    {
        #region IComputationInterface<T> Members

        int IComputationInterface<T>.Length
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() > 0);
                return 0;
            }
        }

        T IComputationInterface<T>.this[int index]
        {
            get 
            {
                var intf = (IComputationInterface<T>)this;
                Contract.Requires(index >= 0 && index < intf.Length);
                return default(T);
            }
            set
            {
                var intf = (IComputationInterface<T>)this;
                Contract.Requires(index >= 0 && index < intf.Length);
            }
        }

        #endregion
    }
}
