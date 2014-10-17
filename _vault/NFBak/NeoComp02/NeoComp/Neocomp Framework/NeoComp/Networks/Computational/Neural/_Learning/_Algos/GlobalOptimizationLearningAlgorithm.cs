using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;

namespace NeoComp.Networks.Computational.Neural
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
