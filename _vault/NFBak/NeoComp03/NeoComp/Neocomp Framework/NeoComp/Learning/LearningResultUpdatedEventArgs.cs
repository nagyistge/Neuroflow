using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Learning
{
    public sealed class LearningResultUpdatedEventArgs : EventArgs
    {
        internal LearningResultUpdatedEventArgs(LearningEpoch epoch, double mse)
        {
            Contract.Requires(epoch != null);
            Contract.Requires(mse >= 0.0);

            Epoch = epoch;
            MSE = mse;
        }

        public LearningEpoch Epoch { get; private set; }

        public double MSE { get; private set; }
    }
}
