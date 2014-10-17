using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.NeuralNetworks
{
    public sealed class ActivationDescription : LayerDescription
    {
        public ActivationDescription() :
            this(ActivationFunction.Sigmoid)
        {
        }
        public ActivationDescription(ActivationFunction function) :
            this(function, function == ActivationFunction.Sigmoid ? 1.75f : 1.0f)
        {
        }

        public ActivationDescription(ActivationFunction function, float alpha)
        {
            Function = function;
            Alpha = alpha;
        }

        public ActivationFunction Function { get; set; }

        public float Alpha { get; set; }
    }
}
