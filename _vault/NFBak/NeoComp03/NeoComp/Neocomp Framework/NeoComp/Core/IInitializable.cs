using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics.Contracts;

namespace NeoComp.Core
{
    [ContractClass(typeof(IInitializableContract))]
    public interface IInitializable
    {
        bool Initialized { get; }

        void Initialize();

        void Uninitialize();
    }

    [ContractClassFor(typeof(IInitializable))]
    class IInitializableContract : IInitializable
    {
        bool IInitializable.Initialized
        {
            get { return false; }
        }

        void IInitializable.Initialize()
        {
            Contract.Ensures(((IInitializable)this).Initialized);
        }

        void IInitializable.Uninitialize()
        {
            Contract.Ensures(((IInitializable)this).Initialized == false);
        }
    }
}
