using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.NeuralNetworks.Learning.Algorithms
{
    public abstract class GradientRule : LearningRule
    {
        protected internal abstract LearningMode GetMode();
    }
}
