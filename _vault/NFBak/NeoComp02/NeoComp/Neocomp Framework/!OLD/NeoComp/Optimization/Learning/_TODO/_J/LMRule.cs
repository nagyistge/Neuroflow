using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.Learning
{
    public sealed class LMRule : BackwardRule
    {
        protected override Type AlgorithmType
        {
            get { return typeof(LMAlgorithm); }
        }

        double lambda = 0.1;

        public double Lambda
        {
            get { return lambda; }
            set
            {
                Contract.Requires(value >= 0.0 && value <= 1.0);

                lambda = value;
            }
        }

        double delta = 5;

        public double Delta
        {
            get { return delta; }
            set
            {
                Contract.Requires(value >= 0.0 && value <= 1.0);

                delta = value;
            }
        }
    }
}
