using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp;

namespace NeoComp.NeuralNetworks.Learning.Algorithms
{
    public abstract class LocalAdaptiveGDRule : GradientDescentRule
    {
        DoubleRange stepSizeRange = new DoubleRange(0.005, 0.5);

        public DoubleRange StepSizeRange
        {
            get { return stepSizeRange; }
            set
            {
                Contract.Requires(!value.IsFixed);

                stepSizeRange = value;
            }
        }

        public bool StochasticAdaptiveStateUpdate { get; set; }
    }
}
