using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Core;
using System.Collections;

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
        
        void WriteValues(IList<T> source, int beginIndex = 0);

        void ReadValues(IList<T> target, int count, int beginIndex = 0);
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

        object ISynchronized.SyncRoot
        {
            get { return null; }
        } 

        #endregion
    }

    [ContractClassFor(typeof(IComputationalInterface<>))]
    sealed class IComputationalInterfaceContract<T> : IComputationalInterface<T>
    {
        void IComputationalInterface<T>.WriteValues(IList<T> source, int beginIndex)
        {
            IComputationalInterface<T> intf = this;
            Contract.Requires(!source.IsNullOrEmpty());
            Contract.Requires(beginIndex >= 0 && beginIndex + source.Count <= intf.Length);
        }

        void IComputationalInterface<T>.ReadValues(IList<T> target, int count, int beginIndex)
        {
            IComputationalInterface<T> intf = this;
            Contract.Requires(target != null);
            Contract.Requires(count > 0);
            Contract.Requires(beginIndex >= 0 && beginIndex + count <= intf.Length);
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

        object ISynchronized.SyncRoot
        {
            get { throw new NotImplementedException(); }
        } 

        #endregion
    }
}
