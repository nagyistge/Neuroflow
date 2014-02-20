using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Neuroflow.Core;
using System.ComponentModel.DataAnnotations;
using Neuroflow.ComponentModel;
using System.ComponentModel;

namespace Neuroflow.Core.NeuralNetworks.Learning.Algorithms
{
    public abstract class LocalAdaptiveGDRule : GradientDescentRule
    {
        protected const double DefMinStepSize = 0.0005;

        protected const double DefMaxStepSize = 0.1;

        protected LocalAdaptiveGDRule()
        {
            MinStepSize = DefMinStepSize;
            MaxStepSize = DefMaxStepSize;
        }

        [Required]
        [InitValue(DefMinStepSize)]
        [DefaultValue(DefMinStepSize)]
        [Category(PropertyCategories.Math)]
        public virtual double MinStepSize { get; set; }

        [Required]
        [InitValue(DefMaxStepSize)]
        [DefaultValue(DefMaxStepSize)]
        [Category(PropertyCategories.Math)]
        public virtual double MaxStepSize { get; set; }

        [Browsable(false)]
        public override double StepSize
        {
            get { return base.StepSize; }
            set { base.StepSize = value; }
        }
        
        internal DoubleRange StepSizeRange
        {
            get { return new DoubleRange(MinStepSize, MaxStepSize); }
        }

        [InitValue(false)]
        [DefaultValue(false)]
        [Category(PropertyCategories.Behavior)]
        public bool StochasticAdaptiveStateUpdate { get; set; }
    }
}
