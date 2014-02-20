using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Computations;
using NeoComp.Core;
using System.Threading;
using System.Runtime.Serialization;

namespace NeoComp.Networks.Computational
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = NeoComp.xmlns)]
    public abstract class ValueComputationalNetwork<T> : ComputationalNetwork<T>, IComputationalUnit<T?>
        where T : struct
    {
        #region Constructor

        protected ValueComputationalNetwork(ComputationalNetworkFactory<T> factory)
            : base(factory)
        {
            Contract.Requires(factory != null);
        } 

        #endregion

        #region Interface

        internal sealed override INetworkInterface<T> CreateInterface(int length)
        {
            return new ValueComputationalNetworkInterface<T>(length, SyncRoot);
        }

        #endregion

        #region T?

        IComputationalInterface<T?> IComputationalUnit<T?>.InputInterface
        {
            get { return (IComputationalInterface<T?>)InputInterface; }
        }

        IComputationalInterface<T?> IComputationalUnit<T?>.OutputInterface
        {
            get { return (IComputationalInterface<T?>)OutputInterface; }
        }

        #endregion

        #region Clone

        new public ValueComputationalNetwork<T> Clone()
        {
            return (ValueComputationalNetwork<T>)base.Clone();
        }

        #endregion
    }
}
