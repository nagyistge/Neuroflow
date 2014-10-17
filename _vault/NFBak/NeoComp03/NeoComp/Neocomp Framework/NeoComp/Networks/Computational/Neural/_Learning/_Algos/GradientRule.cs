using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.Networks.Computational.Neural
{
    public abstract class GradientRule : LearningRule
    {
        protected internal abstract LearningMode GetMode();
    }
}
