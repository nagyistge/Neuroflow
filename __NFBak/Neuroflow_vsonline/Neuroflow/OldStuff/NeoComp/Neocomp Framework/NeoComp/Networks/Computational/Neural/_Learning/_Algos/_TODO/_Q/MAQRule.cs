using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.Networks.Computational.Neural
{
    public sealed class MAQRule : LearningRule
    {
        protected override Type AlgorithmType
        {
            get { return typeof(MAQAlgorithm); }
        }
    }
}
