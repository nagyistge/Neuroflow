using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Neuroflow.Core.Vectors;

namespace Neuroflow.Core.Vectors
{
    [ContractClass(typeof(IComputationUnitContract<>))]
    public interface IComputationUnit<T> : IInterfaced, IReset where T : struct
    {
        object CreateContext();

        void Iteration(object context);

        void WriteInput(object context, BufferedVector<T> values);

        void ReadOutput(object context, T[] values);
    }

    [ContractClassFor(typeof(IComputationUnit<>))]
    abstract class IComputationUnitContract<T> : IComputationUnit<T> where T : struct
    {
        object IComputationUnit<T>.CreateContext()
        {
            Contract.Ensures(Contract.Result<IDisposable>() != null);
            return null;
        }

        void IComputationUnit<T>.Iteration(object context)
        {
            Contract.Requires(context != null);
        }

        void IComputationUnit<T>.WriteInput(object context, BufferedVector<T> values)
        {
            IComputationUnit<T> u = this;
            Contract.Requires(context != null);
            Contract.Requires(values != null);
            Contract.Requires(values.Length == u.InputInterfaceLength);
        }

        void IComputationUnit<T>.ReadOutput(object context, T[] values)
        {
            IComputationUnit<T> u = this;
            Contract.Requires(context != null);
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

        void IReset.Reset(object context)
        {
        }
    }
}
