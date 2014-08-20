using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.Learning
{
    public abstract class UDLocalAdaptiveGDRule : LocalAdaptiveGDRule
    {
        public UDLocalAdaptiveGDRule(double u, double d)
        {
            Contract.Requires(d > 0.0);
            Contract.Requires(u > 0.0);

            U = u;
            D = d;
            StepSize = StepSizeRange.MaxValue;
        }

        double u;

        public double U
        {
            get { return u; }
            set
            {
                Contract.Requires(value > 0.0);

                u = value;
            }
        }

        double d;

        public double D
        {
            get { return d; }
            set
            {
                Contract.Requires(value > 0.0);

                d = value;
            }
        }
    }
}
