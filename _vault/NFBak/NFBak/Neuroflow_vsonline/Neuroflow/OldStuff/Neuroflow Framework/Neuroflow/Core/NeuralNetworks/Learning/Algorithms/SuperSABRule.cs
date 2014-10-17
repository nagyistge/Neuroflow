using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neuroflow.Core.NeuralNetworks.Learning.Algorithms
{
    public sealed class SuperSABRule : UDLocalAdaptiveGDRule
    {
        public SuperSABRule()
            : base(1.1, 0.5)
        {
        }

        protected override Type AlgorithmType
        {
            get { return typeof(SuperSABAlgorithm); }
        }
    }
}
