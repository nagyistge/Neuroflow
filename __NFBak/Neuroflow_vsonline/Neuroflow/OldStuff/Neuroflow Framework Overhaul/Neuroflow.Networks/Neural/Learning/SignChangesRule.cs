using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neuroflow.Networks.Neural.Learning
{
    public sealed class SignChangesRule : AutoAdjustedGradientDescentRule
    {
        public SignChangesRule()
            : base(1.1, 1.0 / 1.1)
        {
        }

        public override Type AlgorithmType
        {
            get { return typeof(SignChangesAlgorithm); }
        }
    }
}
