using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core;

namespace Neuroflow.Core.NeuralNetworks.Learning.Algorithms
{
    public abstract class GlobalOptimizationLearningAlgorithm : BackwardLearningAlgorithm
    {
        protected override bool WantBackpropagation
        {
            get { return false; }
        }

        protected GlobalOptimizationLearningRule Rule
        {
            get { return (GlobalOptimizationLearningRule)LearningConnections.ItemArray[0].Rule; }
        }

        protected DoubleRange WeightRange
        {
            get { return new DoubleRange(-Rule.WeightRange, Rule.WeightRange); }
        }
    }
}
