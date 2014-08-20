using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Computations;
using System.Diagnostics.Contracts;
using NeoComp.Core;

namespace NeoComp.Networks.Neural
{
    public sealed class NeuralNetworkTestResult : ComputationTestResult<double, double?>
    {
        internal NeuralNetworkTestResult(Guid testUID, IList<ComputationTestResultEntry<double, double?>> entryList, TransformedComputation<double, double> computation)
            : base(testUID, entryList)
        {
            Contract.Requires(!entryList.IsNullOrEmpty());
            Contract.Requires(computation != null);
            Contract.Requires(computation.InToOutTransformer is DoubleNormalizer);
            

            Computation = computation;
            var inToOutNorm = (DoubleNormalizer)Computation.InToOutTransformer;
            this.range = inToOutNorm.SourceMax - inToOutNorm.SourceMin;
        }

        double range;

        public TransformedComputation<double, double> Computation { get; private set; }

        protected override double GetError(double computedValue, double? desiredValue)
        {
            return desiredValue.HasValue ? (desiredValue.Value - computedValue) / range : 0.0;
        }
    }
}
