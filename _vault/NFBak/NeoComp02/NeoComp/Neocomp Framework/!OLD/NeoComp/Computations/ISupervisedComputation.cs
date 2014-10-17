using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Computations
{
    [ContractClass(typeof(ISupervisedComputationContract<>))]
    public interface ISupervisedComputation<T>
        where T : struct
    {
        double Scale { get; }
        
        void Compute(Matrix<T> input, Matrix<T> desiredOutput, out Matrix<T> output, bool doAdjustments = true);
    }

    [ContractClassFor(typeof(ISupervisedComputation<>))]
    class ISupervisedComputationContract<T> : ISupervisedComputation<T>
        where T : struct
    {
        void ISupervisedComputation<T>.Compute(Matrix<T> input, Matrix<T> desiredOutput, out Matrix<T> output, bool doAdjustments)
        {
            Contract.Requires(input != null);
            Contract.Requires(desiredOutput != null);
            Contract.Requires(input.Height == desiredOutput.Height);
            Contract.Ensures(Contract.ValueAtReturn(out output) != null);
            Contract.Ensures(Contract.ValueAtReturn(out output).Width == desiredOutput.Width);
            Contract.Ensures(Contract.ValueAtReturn(out output).Height == input.Height);
            output = null;
        }

        double ISupervisedComputation<T>.Scale
        {
            get
            {
                Contract.Ensures(Contract.Result<double>() > 0.0);
                return 0;
            }
        }
    }
}
