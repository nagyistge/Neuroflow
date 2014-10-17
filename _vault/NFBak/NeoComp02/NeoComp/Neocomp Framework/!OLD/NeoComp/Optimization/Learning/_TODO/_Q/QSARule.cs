using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.Optimization.Learning
{
    public sealed class QSARule : BackwardRule
    {
        protected override Type AlgorithmType
        {
            get { return typeof(QSAAlgorithm); }
        }

        protected override bool WantGradientInformation
        {
            get { return false; }
        }
    }
}
