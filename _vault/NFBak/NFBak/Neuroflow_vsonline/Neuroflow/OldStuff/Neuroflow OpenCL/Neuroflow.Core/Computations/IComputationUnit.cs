using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Neuroflow.Core.Computations
{
    [ContractClass(typeof(IComputationUnitContract<>))]
    public interface IComputationUnit<T> : IInterfaced, ISynchronized, IReset where T : struct
    {
        void Iteration();

        void WriteInput(T[] values);

        void ReadOutput(T[] values);
    }

    [ContractClassFor(typeof(IComputationUnit<>))]
    abstract class IComputationUnitContract<T> : IComputationUnit<T> where T : struct
    {
        void IComputationUnit<T>.Iteration()
        {
        }

        void IComputationUnit<T>.WriteInput(T[] values)
        {
            IComputationUnit<T> u = this;
            Contract.Requires(values != null);
            Contract.Requires(values.Length == u.InputInterfaceLength);
        }

        void IComputationUnit<T>.ReadOutput(T[] values)
        {
            IComputationUnit<T> u = this;
            Contract.Requires(values != null);
            Contract.Requires(values.Length == u.OutputInterfaceLength);
        }

        int IInterfaced.InputInterfaceLength
        {
            get { return 0; }
        }

        int IInterfaced.OutputInterfaceLength
        {
            get { return 0; }
        }

        SyncContext ISynchronized.SyncRoot
        {
            get { return null; }
        }

        void IReset.Reset()
        {
        }
    }
}
