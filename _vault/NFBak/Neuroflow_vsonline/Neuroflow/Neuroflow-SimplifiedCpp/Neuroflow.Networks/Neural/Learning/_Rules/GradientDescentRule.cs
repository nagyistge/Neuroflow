using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Neuroflow.Core.Vectors;

namespace Neuroflow.Networks.Neural.Learning
{
    public class GradientDescentRule : ErrorBasedLearningRule
    {
        const double DefStepSize = 0.01;
        const double DefMomentum = 0.8;

        public GradientDescentRule()
        {
            StepSize = DefStepSize;
            Momentum = DefMomentum;
            IterationRepeat = 5;
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
        public double Momentum { get; set; }

        [Required]
        [Category(PropertyCategories.Math)]
        [InitValue(false)]
        [DefaultValue(false)]
        public bool UseSmoothing { get; set; }

        [Required]
        [InitValue(LearningMode.Stochastic)]
        [DefaultValue(LearningMode.Stochastic)]
        [Category(PropertyCategories.Behavior)]
        public LearningMode Mode { get; set; }

        protected internal override LearningMode GetMode()
        {
            return Mode;
        }

        public override bool NeedsGradientInformation
        {
            get { return true; }
        }
    }
}
