using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Neuroflow.Core.ComponentModel;
using System.ComponentModel;

namespace Neuroflow.Networks.Neural
{
    public abstract class ActivationFunction
    {
        protected ActivationFunction(float alpha)
        {
            Contract.Requires(alpha > 0.0);

            Alpha = alpha;
        }

        public float Alpha { get; private set; }

        public abstract float Calculate(float value);

        public abstract float CalculateDerivate(float value);
    }
}
