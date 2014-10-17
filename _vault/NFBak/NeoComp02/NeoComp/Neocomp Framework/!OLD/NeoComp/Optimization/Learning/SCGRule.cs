using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.Optimization.Learning
{
    public sealed class SCGRule : BackwardRule
    {
        protected override Type AlgorithmType
        {
            get { return typeof(SCGAlgorithm); }
        }
    }
}
