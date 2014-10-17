using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Neuroflow.Core.ComponentModel;
using System.ComponentModel;

namespace Neuroflow.Networks.Neural
{
    public sealed class SigmoidActivationFunction : ActivationFunction
    {
        public SigmoidActivationFunction(
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
            return ((2.0f / (1.0f + (float)Math.Exp(-Alpha * value))) - 1.0f);
        }

        public override float CalculateDerivate(float value)
        {
            return (Alpha * (1.0f - value * value) / 2.0f);
        }
    }
}
