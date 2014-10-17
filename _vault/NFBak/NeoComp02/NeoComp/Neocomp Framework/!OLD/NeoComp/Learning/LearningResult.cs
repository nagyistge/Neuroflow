using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Diagnostics.Contracts;

namespace NeoComp.Learning
{
    public sealed class LearningResult : SynchronizedObject
    {
        internal LearningResult(LearningEpoch epoch, bool bestHolder = false)
            : base(epoch.SyncRoot)
        {
            Contract.Requires(epoch != null);

            Epoch = epoch;
            this.bestHolder = bestHolder;
        }

        bool bestHolder;

        public LearningEpoch Epoch { get; private set; }

        double mse = 1.0;

        public double MSE
        {
            get { lock (SyncRoot) return mse; }
        }

        public event EventHandler<LearningResultUpdatedEventArgs> Updated;

        internal void Update(double mse)
        {
            Contract.Requires(mse >= 0.0);

            lock (SyncRoot)
            {
                if (bestHolder && mse >= this.mse) return;
                this.mse = mse;
                var handler = Updated;
                if (handler != null) handler(this, new LearningResultUpdatedEventArgs(Epoch, mse));
            }
        }
    }
}
