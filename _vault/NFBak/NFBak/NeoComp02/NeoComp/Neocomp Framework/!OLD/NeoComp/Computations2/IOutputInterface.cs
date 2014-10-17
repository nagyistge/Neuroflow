using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Computations2
{
    [ContractClass(typeof(IOutputInterfaceContract<>))]
    public interface IOutputInterface<T> : IComputationInterface<T>
        where T : struct
    {
        void Read(T?[] outputBuffer);

        void Read(T[] outputBuffer);
    }

    [ContractClassFor(typeof(IOutputInterface<>))]
    class IOutputInterfaceContract<T> : IOutputInterface<T>
        where T : struct
    {
        #region IOutputInterface<T> Members

        void IOutputInterface<T>.Read(T?[] outputBuffer)
        {
            var intf = (IOutputInterface<T>)this;
            Contract.Requires(outputBuffer != null && outputBuffer.Length == intf.Length);
        }

        void IOutputInterface<T>.Read(T[] outputBuffer)
        {
            var intf = (IOutputInterface<T>)this;
            Contract.Requires(outputBuffer != null && outputBuffer.Length == intf.Length);
        }

        #endregion

        #region IComputationInterface<T> Members

        int IComputationInterface<T>.Length
        {
            get { return 0; }
        }

        T IComputationInterface<T>.this[int index]
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}
