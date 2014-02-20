using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Computations
{
    [ContractClass(typeof(IUnsupervisedComputationContract<>))]
    public interface IUnsupervisedComputation<T>
        where T : struct
    {
        void Compute(Matrix<T> input, out Matrix<T> output, bool doAdjustments = true);
    }

    [ContractClassFor(typeof(IUnsupervisedComputation<>))]
    class IUnsupervisedComputationContract<T> : IUnsupervisedComputation<T>
        where T : struct
    {
        void IUnsupervisedComputation<T>.Compute(Matrix<T> input, out Matrix<T> output, bool doAdjustments)
        {
            Contract.Requires(input != null);
            Contract.Ensures(Contract.ValueAtReturn(out output) != null);
            Contract.Ensures(Contract.ValueAtReturn(out output).Height == input.Height);
            output = null;
        }
    }
}
