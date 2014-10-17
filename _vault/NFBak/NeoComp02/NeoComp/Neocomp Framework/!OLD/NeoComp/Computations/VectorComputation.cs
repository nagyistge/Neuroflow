using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Threading;

namespace NeoComp.Computations
{
    public class Learningutation<T> : Computation<T?>
        where T : struct
    {
        public Learningutation(int numberOfIterations = 1)
            : base(numberOfIterations)
        {
            Contract.Requires(numberOfIterations > 0);
        }

        public Vector<T> Compute(IComputationalUnit<T?> compUnit, Vector<T> inputVector, CancellationToken? cancellationToken = null)
        {
            Contract.Requires(compUnit != null);
            Contract.Requires(inputVector != null);
            Contract.Requires(inputVector.Dimension > 0);

            var outputBuffer = new T?[compUnit.OutputInterface.Length];
            ComputeInternal(compUnit, inputVector.ItemArray, outputBuffer, cancellationToken);
            return Vector.Wrap(outputBuffer);
        }
    }
}
