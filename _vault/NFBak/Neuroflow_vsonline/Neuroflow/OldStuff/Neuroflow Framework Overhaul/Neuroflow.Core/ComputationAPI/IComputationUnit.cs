using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Neuroflow.Core;

namespace Neuroflow.Core.ComputationAPI
{
    [ContractClass(typeof(IComputationUnitContract<>))]
    public interface IComputationUnit<T> : IInterfaced, ISynchronized, IDisposable, IReset where T : struct
    {
        ComputationInterface<T> InputInterface { get; }

        ComputationInterface<T> OutputInterface { get; }
        
        void Iteration();
    }

    [ContractClassFor(typeof(IComputationUnit<>))]
    abstract class IComputationUnitContract<T> : IComputationUnit<T> where T : struct
    {
        void IComputationUnit<T>.Iteration()
        {
        }

        ComputationInterface<T> IComputationUnit<T>.InputInterface
        {
            get
            {
                Contract.Ensures(Contract.Result<ComputationInterface<T>>() != null);
                return null;
            }
        }

        ComputationInterface<T> IComputationUnit<T>.OutputInterface
        {
            get
            {
                Contract.Ensures(Contract.Result<ComputationInterface<T>>() != null);
                return null;
            }
        }

        int IInterfaced.InputInterfaceLength
        {
            get { throw new NotImplementedException(); }
        }

        int IInterfaced.OutputInterfaceLength
        {
            get { throw new NotImplementedException(); }
        }

        SyncContext ISynchronized.SyncRoot
        {
            get { throw new NotImplementedException(); }
        }

        void IDisposable.Dispose()
        {
        }

        void IReset.Reset()
        {
        }
    }
}
