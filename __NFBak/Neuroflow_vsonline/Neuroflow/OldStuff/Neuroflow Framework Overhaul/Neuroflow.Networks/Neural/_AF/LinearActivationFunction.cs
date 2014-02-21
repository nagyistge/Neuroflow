using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core.ComputationAPI;
using System.Diagnostics.Contracts;

namespace Neuroflow.Networks.Neural
{
    public sealed class LinearActivationFunction : ActivationFunction
    {
        public LinearActivationFunction()
            : base()
        {
        }
        
        public LinearActivationFunction(double alpha)
            : base(alpha)
        {
            Contract.Requires(alpha > 0.0);
        }
        
        public override string Function(ComputationBlock block, string value)
        {
            block.Add("double lafResult = ((" + value + ") * " + Alpha + ")");
            block.Add("if (lafResult < -" + Alpha + ") lafResult = -" + Alpha + "; else if (lafResult > " + Alpha + ") lafResult = " + Alpha + ";");
            return "lafResult";
        }

        public override string Derivate(ComputationBlock block, string value)
        {
            return Alpha.ToString();
        }
    }
}
