using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Diagnostics.Contracts;

namespace NeoComp.Networks.Computational.Neural
{
    public sealed class RpropRule : UDLocalAdaptiveGDRule
    {
        public RpropRule()
            : base(1.2, 0.5)
        {
            StepSizeRange = new DoubleRange(-50.0, 50.0);
        }

        protected override Type AlgorithmType
        {
            get { return typeof(RpropAlgorithm); }
        }
    }
}
