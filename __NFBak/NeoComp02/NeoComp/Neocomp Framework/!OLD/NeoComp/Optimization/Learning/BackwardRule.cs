using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks.Computational.Neural;

namespace NeoComp.Optimization.Learning
{
    public abstract class BackwardRule : LearningRule, IBackwardRule
    {
        protected virtual bool WantForwardIteration
        {
            get { return false; }
        }

        protected virtual bool WantGradientInformation
        {
            get { return true; }
        }
        
        bool IBackwardRule.WantForwardIteration
        {
            get { return WantForwardIteration; }
        }

        bool IBackwardRule.WantGradientInformation
        {
            get { return WantGradientInformation ; }
        }
    }
}
