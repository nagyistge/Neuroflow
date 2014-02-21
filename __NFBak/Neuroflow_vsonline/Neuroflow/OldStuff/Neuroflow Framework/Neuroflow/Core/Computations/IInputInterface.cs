using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Neuroflow.Core.Computations
{
    [ContractClass(typeof(IInputInterfaceContract<>))]
    public interface IInputInterface<T> : IComputationInterface<T>
        where T : struct
    {
        void Write(T?[] inputBuffer);

        void Write(T[] inputBuffer);
    }

    [ContractClassFor(typeof(IInputInterface<>))]
    class IInputInterfaceContract<T> : IInputInterface<T>
        where T : struct
    {
        #region IInputInterface<T> Members

        void IInputInterface<T>.Write(T?[] inputBuffer)
        {
            var intf = (IInputInterface<T>)this;
            Contract.Requires(inputBuffer != null && inputBuffer.Length == intf.Length);
        }

        void IInputInterface<T>.Write(T[] inputBuffer)
        {
            var intf = (IInputInterface<T>)this;
            Contract.Requires(inputBuffer != null && inputBuffer.Length == intf.Length);
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
            set { throw new NotImplementedException(); }
        }

        #endregion
    }
}
