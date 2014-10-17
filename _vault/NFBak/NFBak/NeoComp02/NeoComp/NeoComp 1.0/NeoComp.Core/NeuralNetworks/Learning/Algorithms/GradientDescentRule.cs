using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.NeuralNetworks.Learning.Algorithms
{
    public class GradientDescentRule : MultimodeGradientRule, IWeightDecayedLearningRule
    {
        double stepSize = 0.1;

        public double StepSize
        {
            get { return stepSize; }
            set
            {
                Contract.Requires(value >= 0.0);

                stepSize = value;
            }
        }

        double momentum = 0.8;

        public double Momentum
        {
            get { return momentum; }
            set
            {
                Contract.Requires(value >= 0.0 && value < 1.0);

                momentum = value;
            }
        }

        protected override Type AlgorithmType
        {
            get { return typeof(GradientDescentAlgorithm); }
        }

        public WeightDecay WeightDecay { get; set; }
    }
}
