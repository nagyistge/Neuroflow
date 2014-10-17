using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.Learning
{
    public sealed class RpropRule : UDLocalAdaptiveGDRule
    {
        public RpropRule()
            : base(1.2, 0.5)
        {
            StepSizeRange = new DoubleRange(0.000001, 50.0);
        }

        protected override Type AlgorithmType
        {
            get { return typeof(RpropAlgorithm); }
        }
    }
}
