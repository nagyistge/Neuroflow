using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks.Computational.Neural;

namespace NeoComp.Optimization.Learning
{
    public abstract class ForwardLearningAlgorithm : LearningAlgorithm
    {
        protected internal sealed override void BackwardIteration(bool batch, double mse)
        {
            throw new NotSupportedException(this + " BackwardIteration is not supported.");
        }
    }
}
