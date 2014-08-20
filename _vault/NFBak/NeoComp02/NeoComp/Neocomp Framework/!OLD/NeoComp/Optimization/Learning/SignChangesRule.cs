using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.Learning
{
    public sealed class SignChangesRule : UDLocalAdaptiveGDRule
    {
        public SignChangesRule()
            : base(1.1, 1.0 / 1.1)
        {
        }
        
        protected override Type AlgorithmType
        {
            get { return typeof(SignChangesAlgorithm); }
        }
    }
}
