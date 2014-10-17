using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.SupervisedLearning
{
    public abstract class RuleParameters : IRuleParameters
    {
        double weightInititlizationNoise = 1.0;

        public double WeightInititlizationNoise
        {
            get { return weightInititlizationNoise; }
            set
            {
                Contract.Requires(value >= 0 && value <= 1.0);
                weightInititlizationNoise = value;
            }
        }
    }
}
