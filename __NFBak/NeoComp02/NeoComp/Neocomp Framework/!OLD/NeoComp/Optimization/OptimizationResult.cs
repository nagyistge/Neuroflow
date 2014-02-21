using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Diagnostics.Contracts;
using NeoComp.Features;
using NeoComp.Computations;

namespace NeoComp.Optimization
{
    public sealed class OptimizationResult : SynchronizedObject
    {
        internal OptimizationResult(OptimizationEpoch epoch, bool bestHolder = false)
            : base(epoch.SyncRoot)
        {
            Contract.Requires(epoch != null);

            Epoch = epoch;
            this.bestHolder = bestHolder;
        }

        bool bestHolder;

        public OptimizationEpoch Epoch { get; private set; }

        double mse = 1.0;

        public double MSE
        {
            get { lock (SyncRoot) return mse; }
        }

        public event EventHandler<OptimizationResultUpdatedEventArgs> Updated;

        internal void Update(double mse, FeatureMatrix featureMatrix, Matrix<double> output)
        {
            Contract.Requires(mse >= 0.0);
            Contract.Requires(featureMatrix != null);
            Contract.Requires(output != null);

            lock (SyncRoot)
            {
                if (bestHolder && mse >= this.mse) return;
                this.mse = mse;
                var handler = Updated;
                if (handler != null) handler(this, new OptimizationResultUpdatedEventArgs(Epoch, mse, featureMatrix, output));
            }
        }
    }
}
