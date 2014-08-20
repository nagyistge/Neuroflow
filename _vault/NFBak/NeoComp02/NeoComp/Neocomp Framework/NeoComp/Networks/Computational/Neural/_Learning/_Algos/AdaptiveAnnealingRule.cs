using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Networks.Computational.Neural
{
    public sealed class AdaptiveAnnealingRule : GlobalOptimizationLearningRule
    {
        protected override Type AlgorithmType
        {
            get { return typeof(AdaptiveAnnealingAlgorithm); }
        }

        double weightGenMul = 1.0;

        public double WeightGenMul
        {
            get { return weightGenMul; }
            set
            {
                Contract.Requires(value > 0);

                weightGenMul = value;
            }
        }

        double acceptProbMul = 1.0;

        public double AcceptProbMul
        {
            get { return acceptProbMul; }
            set
            {
                Contract.Requires(value > 0);

                acceptProbMul = value;
            }
        }
    }
}
