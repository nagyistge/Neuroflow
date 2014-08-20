using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.NeuralNetworks.Learning.Algorithms
{
    public abstract class GlobalOptimizationLearningRule : LearningRule
    {
        double weightRange = 1.1;

        public double WeightRange
        {
            get { return weightRange; }
            set
            {
                Contract.Requires(value > 0.0);
                weightRange = value;
            }
        }
    }
}
