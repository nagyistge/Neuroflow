using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Computations;
using NeoComp.Core;

namespace NeoComp.Networks.Computational
{
    [ContractClass(typeof(INetworkInterfaceContract<>))]
    internal interface INetworkInterface<T> : IComputationalInterface<T>
    {
        ComputationalValue<T> GetComputationalValue(int index);

        void SetValueAt(int index, T value);
    }

    [ContractClassFor(typeof(INetworkInterface<>))]
    class INetworkInterfaceContract<T> : INetworkInterface<T>
    {
        ComputationalValue<T> INetworkInterface<T>.GetComputationalValue(int index)
        {
            INetworkInterface<T> bd = this;
            Contract.Requires(index >= 0 && index < bd.Length);
            return null;
        }

        void INetworkInterface<T>.SetValueAt(int index, T value)
        {
            INetworkInterface<T> bd = this; 
            Contract.Requires(index >= 0 && index < bd.Length);
        }

        #region Dummy

        T IComputationalInterface<T>.this[int index]
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

        void IComputationalInterface<T>.WriteValues(T[] source, int beginIndex)
        {
            throw new NotImplementedException();
        }

        void IComputationalInterface<T>.ReadValues(T[] target, int beginIndex)
        {
            throw new NotImplementedException();
        }

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
