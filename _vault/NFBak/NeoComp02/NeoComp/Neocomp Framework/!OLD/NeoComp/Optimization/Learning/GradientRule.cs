using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.Optimization.Learning
{
    public abstract class GradientRule : BackwardRule
    {
        protected internal abstract LearningMode GetMode();
    }
}
