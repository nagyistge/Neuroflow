using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neuroflow.Networks.Neural.Learning
{
    public abstract class ForwardLearningRule : LearningRule
    {
        public sealed override bool IsBeforeIterationRule
        {
            get { return true; }
        }

        public override bool NeedsGradientInformation
        {
            get { return false; }
        }
    }
}
