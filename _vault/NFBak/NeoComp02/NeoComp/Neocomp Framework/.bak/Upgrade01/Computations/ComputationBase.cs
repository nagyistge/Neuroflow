using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Core;

namespace NeoComp.Computations
{
    public abstract class ComputationBase<T>
    {
        protected ComputationBase(ComputationEpoch<T> epoch)
        {
            Contract.Requires(epoch != null);

            Epoch = epoch;
        }

        public ComputationEpoch<T> Epoch { get; private set; }

        protected IList<T> ComputeOutput(IComputationalUnit<T> computationUnit, IList<T> inputValues)
        {
            Contract.Requires(!inputValues.IsNullOrEmpty());
            Contract.Requires(computationUnit != null);

            var result = new List<T>();
            Epoch.Run(computationUnit, inputValues, result);
            return result.AsReadOnly();
        }
    }
}
