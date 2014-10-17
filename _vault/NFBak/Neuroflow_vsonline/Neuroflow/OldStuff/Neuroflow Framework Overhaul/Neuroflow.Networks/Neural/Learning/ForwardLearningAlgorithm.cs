using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neuroflow.Networks.Neural.Learning
{
    public abstract class ForwardLearningAlgorithm<TRule> : LearningAlgorithm
        where TRule : LearningRule
    {
        #region Props

        new protected TRule Rule
        {
            get { return (TRule)base.Rule; }
        }

        public override bool WantForwardIteration
        {
            get { return true; }
        }

        public override BackwardIterationMode BackwardIterationMode
        {
            get { return BackwardIterationMode.Disabled; }
        }

        #endregion
    }
}
