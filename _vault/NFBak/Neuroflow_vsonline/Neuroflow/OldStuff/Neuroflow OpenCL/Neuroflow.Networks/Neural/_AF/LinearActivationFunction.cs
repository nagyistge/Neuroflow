using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Neuroflow.Core.ComponentModel;
using System.ComponentModel;

namespace Neuroflow.Networks.Neural
{
    public sealed class LinearActivationFunction : ActivationFunction
    {
        public LinearActivationFunction(
            [InitValue(1.0)]
            [DefaultValue(1.0)]
            [Category(PropertyCategories.Math)]
            [FreeDisplayName("Alpha")]
            float alpha = 1.0f)
            : base(alpha)
        {
            Contract.Requires(alpha > 0.0);
        }

        public override float Calculate(float value)
        {
            float result = (value * Alpha);
            if (result < -Alpha) return -Alpha; else if (result > Alpha) return Alpha;
            return result;
        }

        public override float CalculateDerivate(float value)
        {
            return Alpha;
        }
    }
}
