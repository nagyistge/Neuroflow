using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks.Computational.Neural;

namespace NeoComp.Optimization.Learning
{
    public abstract class BackwardLearningAlgorithm : LearningAlgorithm
    {
        protected double CurrentMSE { get; private set; }

        protected internal override void InitializeNewRun()
        {
            CurrentMSE = double.MaxValue;
        }

        protected internal override void BackwardIteration(bool batch, double mse)
        {
            CurrentMSE = mse;
        }
        
        protected internal override bool ForwardIteration(bool isNewBatch)
        {
            throw new NotSupportedException(this + " ForwardIteration is not supported.");
        }
    }
}
