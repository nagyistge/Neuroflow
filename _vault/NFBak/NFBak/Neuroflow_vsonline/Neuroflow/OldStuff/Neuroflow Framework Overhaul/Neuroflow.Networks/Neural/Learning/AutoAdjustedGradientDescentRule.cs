using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.ComponentModel.DataAnnotations;
using Neuroflow.Core.ComponentModel;
using System.ComponentModel;

namespace Neuroflow.Networks.Neural.Learning
{
    public abstract class AutoAdjustedGradientDescentRule : LocalAdaptiveGradientDescentRule
    {
        protected AutoAdjustedGradientDescentRule(double u, double d)
            : base()
        {
            Contract.Requires(d > 0.0);
            Contract.Requires(u > 0.0);

            U = u;
            D = d;
        }

        public virtual double U { get; private set; }

        public virtual double D { get; private set; }
    }
}
