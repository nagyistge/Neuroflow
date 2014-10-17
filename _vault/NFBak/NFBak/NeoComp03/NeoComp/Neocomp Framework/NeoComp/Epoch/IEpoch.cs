using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Core;

namespace NeoComp.Epoch
{
    [ContractClass(typeof(IEpochContract))]
    public interface IEpoch : ISynchronized, IInitializable
    {
        int CurrentIteration { get; }
        
        void Step();
    }

    [ContractClassFor(typeof(IEpoch))]
    class IEpochContract : IEpoch
    {
        int IEpoch.CurrentIteration
        {
            get { return 0; }
        }

        void IEpoch.Step()
        {
            Contract.Ensures(((IEpoch)this).Initialized);
            Contract.Ensures(((IEpoch)this).CurrentIteration >= Contract.OldValue<int>(((IEpoch)this).CurrentIteration));
        }

        bool IInitializable.Initialized
        {
            get { return false; }
        }

        void IInitializable.Initialize()
        {
            IEpoch e = this;
            Contract.Ensures(e.CurrentIteration == 0);
        }

        void IInitializable.Uninitialize()
        {
            throw new NotImplementedException();
        }

        SyncContext ISynchronized.SyncRoot
        {
            get { throw new NotImplementedException(); }
        }
    }
}
