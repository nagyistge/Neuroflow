using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks.Computational.Neural;

namespace NeoComp.Networks.Computational.Neural
{
    public abstract class ForwardLearningAlgorithm : LearningAlgorithm
    {
        public sealed override BackwardIterationMode BackwardIterationMode
        {
            get { return BackwardIterationMode.None; }
        }

        public sealed override bool WantForwardIteration
        {
            get { return true; }
        }
        
        protected internal sealed override void BackwardIteration(bool batch, double mse)
        {
            throw new NotSupportedException(this + " BackwardIteration is not supported.");
        }
    }
}
