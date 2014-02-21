using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Computations
{
    [ContractClass(typeof(IComputationUnitContract<>))]
    public interface IComputationUnit<T>
        where T : struct
    {
        IInputInterface<T> InputInterface { get; }

        IOutputInterface<T> OutputInterface { get; }

        bool IsFeedForward { get; }

        void Iteration();
    }

    [ContractClassFor(typeof(IComputationUnit<>))]
    class IComputationUnitContract<T> : IComputationUnit<T>
        where T : struct
    {
        #region IComputationUnit Members

        IInputInterface<T> IComputationUnit<T>.InputInterface
        {
            get 
            {
                Contract.Ensures(Contract.Result<IInputInterface<T>>() != null);
                return null; 
            }
        }

        IOutputInterface<T> IComputationUnit<T>.OutputInterface
        {
            get
            {
                Contract.Ensures(Contract.Result<IOutputInterface<T>>() != null);
                return null;
            }
        }

        bool IComputationUnit<T>.IsFeedForward
        {
            get { return false; }
        }

        void IComputationUnit<T>.Iteration()
        {
        }

        #endregion
    }
}
