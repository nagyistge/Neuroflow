using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using Neuroflow.Core.ComponentModel;
using System.ComponentModel;
using Neuroflow.Core;

namespace Neuroflow.Networks.Neural.Learning
{
    public abstract class LocalAdaptiveGradientDescentRule : GradientDescentRule
    {
        protected const double DefMinStepSize = 0.0005;

        protected const double DefMaxStepSize = 0.1;

        protected LocalAdaptiveGradientDescentRule()
        {
            MinStepSize = DefMinStepSize;
            MaxStepSize = DefMaxStepSize;
        }

        [Required]
        [InitValue(DefMinStepSize)]
        [DefaultValue(DefMinStepSize)]
        [Category(PropertyCategories.Math)]
        public double MinStepSize { get; set; }

        [Required]
        [InitValue(DefMaxStepSize)]
        [DefaultValue(DefMaxStepSize)]
        [Category(PropertyCategories.Math)]
        public double MaxStepSize { get; set; }

        [Browsable(false)]
        public override double StepSize
        {
            get { return base.StepSize; }
            set { base.StepSize = value; }
        }

        [InitValue(false)]
        [DefaultValue(false)]
        [Category(PropertyCategories.Behavior)]
        public bool StochasticAdaptiveStateUpdate { get; set; }

        public DoubleRange StepSizeRange
        {
            get { return new DoubleRange(MinStepSize, MaxStepSize); }
        }

        public double InitialStepSize
        {
            get { return StepSizeRange.MinValue + StepSizeRange.Size / 2.0; }
        }
    }
}
