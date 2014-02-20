using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Neuroflow.ComponentModel;

namespace Neuroflow.Core.NeuralNetworks.Learning.Algorithms
{
    public class GradientDescentRule : MultimodeGradientRule, IWeightDecayedLearningRule
    {
        const double DefStepSize = 0.01;
        const double DefMomentum = 0.8;
        
        public GradientDescentRule()
        {
            StepSize = DefStepSize;
            Momentum = DefMomentum;
        }
        
        [Required]
        [InitValue(DefStepSize)]
        [DefaultValue(DefStepSize)]
        [Category(PropertyCategories.Math)]
        public virtual double StepSize { get; set; }

        [Required]
        [InitValue(DefMomentum)]
        [DefaultValue(DefMomentum)]
        [Category(PropertyCategories.Math)]
        public virtual double Momentum { get; set; }

        protected override Type AlgorithmType
        {
            get { return typeof(GradientDescentAlgorithm); }
        }

        [Category(PropertyCategories.Algorithm)]
        public WeightDecay WeightDecay { get; set; }
    }
}
