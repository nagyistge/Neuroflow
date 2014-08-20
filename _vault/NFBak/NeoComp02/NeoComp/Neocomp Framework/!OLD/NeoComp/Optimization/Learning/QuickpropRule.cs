using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Core;

namespace NeoComp.Optimization.Learning
{
    public sealed class QuickpropRule : GradientRule
    {
        public QuickpropRule()
        {
            Mu = 1.75;
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

        double stepSize = 0.01;

        public double StepSize
        {
            get { return stepSize; }
            set
            {
                Contract.Requires(value >= 0.0);

                stepSize = value;
            }
        }

        double mu;

        public double Mu
        {
            get { return mu; }
            set
            {
                Contract.Requires(value > 0.0);

                mu = value;
                Shrink = value / (1.0 + value);
            }
        }

        double modeSwitchThreshold;

        public double ModeSwitchThreshold
        {
            get { return modeSwitchThreshold; }
            set
            {
                Contract.Requires(value >= 0.0);

                modeSwitchThreshold = value;
            }
        }

        internal double Shrink { get; private set; }

        protected override Type AlgorithmType
        {
            get { return typeof(QuickpropAlgorithm); }
        }

        protected internal override LearningMode GetMode()
        {
            return LearningMode.Batch;
        }
    }
}
