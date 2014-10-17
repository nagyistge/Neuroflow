using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Neuroflow.ComponentModel;

namespace Neuroflow.Core.NeuralNetworks.Learning.Algorithms
{
    public sealed class SCGRule : LearningRule, IWeightDecayedLearningRule
    {
        [Category(PropertyCategories.Algorithm)]
        public WeightDecay WeightDecay { get; set; }
        
        protected override Type AlgorithmType
        {
            get { return typeof(SCGAlgorithm); }
        }
    }
}
