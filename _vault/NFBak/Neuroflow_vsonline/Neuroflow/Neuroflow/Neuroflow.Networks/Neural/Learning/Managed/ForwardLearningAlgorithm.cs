using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neuroflow.Networks.Neural.Learning.Managed
{
    public abstract class ForwardLearningAlgorithm<T> : LearningAlgorithm
        where T : ForwardLearningRule
    {
        new public T Rule
        {
            get { return (T)base.Rule; }
        }
    }
}
