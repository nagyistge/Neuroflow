using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Core;

namespace NeoComp.Computations
{
    public class TransformedComputation<TOutter, TInner> : ComputationBase<TInner>
    {
        public TransformedComputation(ComputationEpoch<TInner> epoch, ITransformer<TOutter, TInner> outToInTransformer, ITransformer<TInner, TOutter> inToOutTransformer)
            : base(epoch)
        {
            Contract.Requires(epoch != null);
            Contract.Requires(outToInTransformer != null);
            Contract.Requires(inToOutTransformer != null);

            OutToInTransformer = outToInTransformer;
            InToOutTransformer = inToOutTransformer;
        }

        public ITransformer<TOutter, TInner> OutToInTransformer { get; private set; }

        public ITransformer<TInner, TOutter> InToOutTransformer { get; private set; }

        public IList<TOutter> ComputeOutput(IComputationalUnit<TInner> computationUnit, IList<TOutter> inputValues)
        {
            Contract.Requires(!inputValues.IsNullOrEmpty());
            Contract.Requires(computationUnit != null);

            var innerInput = inputValues.Transform(OutToInTransformer).ToList();
            var innerOutput = base.ComputeOutput(computationUnit, innerInput);
            var result = innerOutput.Transform(InToOutTransformer).ToList().AsReadOnly();

            return result;
        }
    }
}
