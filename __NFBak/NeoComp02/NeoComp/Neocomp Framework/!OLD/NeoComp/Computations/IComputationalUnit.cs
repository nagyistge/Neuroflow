using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Threading;

namespace NeoComp.Computations
{
    [ContractClass(typeof(IComputationalUnitContract<>))]
    public interface IComputationalUnit<T>
    {
        void ComputeOutput(CancellationToken? cancellationToken);

        IComputationalInterface<T> InputInterface { get; }

        IComputationalInterface<T> OutputInterface { get; }
    }

    [ContractClassFor(typeof(IComputationalUnit<>))]
    sealed class IComputationalUnitContract<T> : IComputationalUnit<T>
    {
        void IComputationalUnit<T>.ComputeOutput(CancellationToken? cancellationToken)
        {
        }

        IComputationalInterface<T> IComputationalUnit<T>.InputInterface
        {
            get
            {
                Contract.Ensures(Contract.Result<IComputationalInterface<T>>() != null);
                return null;
            }
        }

        IComputationalInterface<T> IComputationalUnit<T>.OutputInterface
        {
            get
            {
                Contract.Ensures(Contract.Result<IComputationalInterface<T>>() != null);
                return null;
            }
        }
    }
}
