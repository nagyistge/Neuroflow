using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core.ComputationAPI;
using System.Diagnostics.Contracts;

namespace Neuroflow.Networks.Neural
{
    public sealed class SigmoidActivationFunction : ActivationFunction
    {
        public SigmoidActivationFunction()
            : base()
        {
        }
        
        public SigmoidActivationFunction(double alpha)
            : base(alpha)
        {
            Contract.Requires(alpha > 0.0);
        }

        public override string Function(ComputationBlock block, string value)
        {
            block.AddReference(typeof(Math));

            return "((2.0 / (1.0 + Math.Exp(-" + Alpha + " * (" + value + ")))) - 1.0)";
        }

        public override string Derivate(ComputationBlock block, string value)
        {
            return "(" + Alpha + " * (1.0 - (" + value + ") * (" + value + ")) / 2.0)";
        }
    }
}
