using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Core;

namespace NeoComp.Computations
{
    public class Computation<T> : ComputationBase<T>
    {
        public Computation(ComputationEpoch<T> epoch)
            : base(epoch)
        {
            Contract.Requires(epoch != null);
        }

        new public IList<T> ComputeOutput(IComputationalUnit<T> computationUnit, IList<T> inputValues)
        {
            Contract.Requires(!inputValues.IsNullOrEmpty());
            Contract.Requires(computationUnit != null);

            return base.ComputeOutput(computationUnit, inputValues);
        }
    }
}
