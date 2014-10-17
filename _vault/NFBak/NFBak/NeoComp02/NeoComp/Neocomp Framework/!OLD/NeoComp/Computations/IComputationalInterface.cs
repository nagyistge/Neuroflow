using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Core;

namespace NeoComp.Computations
{
    [ContractClass(typeof(IDataInterfaceContract))]
    public interface IComputationalInterface : ISynchronized
    {
        int Length { get; }

        object this[int index] { get; set; }
    }

    [ContractClass(typeof(IComputationalInterfaceContract<>))]
    public interface IComputationalInterface<T> : IComputationalInterface
    {
        new T this[int index] { get; set; }

        void WriteValues(T[] source, int beginIndex = 0);

        void ReadValues(T[] target, int beginIndex = 0);
    }

    [ContractClassFor(typeof(IComputationalInterface))]
    sealed class IDataInterfaceContract : IComputationalInterface
    {
        int IComputationalInterface.Length
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() > 0);
                return 0;
            }
        }

        object IComputationalInterface.this[int index]
        {
            get
            {
                Contract.Requires(index >= 0 && index <= ((IComputationalInterface)this).Length);
                return null;
            }
            set
            {
                Contract.Requires(index >= 0 && index <= ((IComputationalInterface)this).Length);
            }
        }

        #region Dummy

        SyncContext ISynchronized.SyncRoot
        {
            get { return null; }
        }

        #endregion
    }

    [ContractClassFor(typeof(IComputationalInterface<>))]
    sealed class IComputationalInterfaceContract<T> : IComputationalInterface<T>
    {
        void IComputationalInterface<T>.WriteValues(T[] source, int beginIndex)
        {
            IComputationalInterface<T> intf = this;
            Contract.Requires(source != null);
            Contract.Requires(source.Length > 0);
            Contract.Requires(beginIndex >= 0 && beginIndex + source.Length <= intf.Length);
        }

        void IComputationalInterface<T>.ReadValues(T[] target, int beginIndex)
        {
            IComputationalInterface<T> intf = this;
            Contract.Requires(target != null);
            Contract.Requires(target.Length > 0);
            Contract.Requires(beginIndex >= 0 && beginIndex + target.Length <= intf.Length);
        }

        T IComputationalInterface<T>.this[int index]
        {
            get
            {
                IComputationalInterface<T> intf = this;
                Contract.Requires(index >= 0 && index <= intf.Length);
                return default(T);
            }
            set
            {
                IComputationalInterface<T> intf = this;
                Contract.Requires(index >= 0 && index <= intf.Length);
            }
        }

        #region Dummy

        int IComputationalInterface.Length
        {
            get { throw new NotImplementedException(); }
        }

        object IComputationalInterface.this[int index]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        SyncContext ISynchronized.SyncRoot
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}
