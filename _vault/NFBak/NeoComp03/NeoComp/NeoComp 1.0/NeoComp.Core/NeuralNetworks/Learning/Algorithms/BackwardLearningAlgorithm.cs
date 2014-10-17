using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.NeuralNetworks;

namespace NeoComp.NeuralNetworks.Learning.Algorithms
{
    public abstract class BackwardLearningAlgorithm : LearningAlgorithm
    {
        public sealed override BackwardIterationMode BackwardIterationMode
        {
            get { return WantBackpropagation ? BackwardIterationMode.BackPropagation : BackwardIterationMode.Enabled; }
        }

        protected abstract bool WantBackpropagation { get; }

        public override bool WantForwardIteration
        {
            get { return false; }
        }
        
        protected double CurrentMSE { get; private set; }

        protected internal override void InitializeNewRun()
        {
            CurrentMSE = double.MaxValue;
        }

        protected internal override void BackwardIteration(bool batch, double mse)
        {
            CurrentMSE = mse;
        }
        
        protected internal override void ForwardIteration(bool isNewBatch)
        {
            throw new NotSupportedException(this + " ForwardIteration is not supported.");
        }
    }
}
