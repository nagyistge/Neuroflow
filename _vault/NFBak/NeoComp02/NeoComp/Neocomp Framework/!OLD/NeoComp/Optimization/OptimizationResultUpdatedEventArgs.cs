using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Features;
using NeoComp.Computations;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization
{
    public sealed class OptimizationResultUpdatedEventArgs : EventArgs
    {
        internal OptimizationResultUpdatedEventArgs(OptimizationEpoch epoch, double mse, FeatureMatrix featureMatrix, Matrix<double> output)
        {
            Contract.Requires(epoch != null);
            Contract.Requires(mse >= 0.0);
            Contract.Requires(featureMatrix != null);
            Contract.Requires(output != null);

            Epoch = epoch;
            MSE = mse;
            FeatureMatrix = featureMatrix;
            ComputedOutputMatrix = output;
        }

        public OptimizationEpoch Epoch { get; private set; }

        public double MSE { get; private set; }

        public FeatureMatrix FeatureMatrix { get; private set; }

        public Matrix<double> ComputedOutputMatrix { get; private set; }
    }
}
