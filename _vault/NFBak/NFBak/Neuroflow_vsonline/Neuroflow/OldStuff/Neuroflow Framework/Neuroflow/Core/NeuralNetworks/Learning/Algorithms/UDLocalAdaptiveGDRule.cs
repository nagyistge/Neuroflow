using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.ComponentModel.DataAnnotations;
using Neuroflow.ComponentModel;
using System.ComponentModel;

namespace Neuroflow.Core.NeuralNetworks.Learning.Algorithms
{
    public abstract class UDLocalAdaptiveGDRule : LocalAdaptiveGDRule
    {
        protected UDLocalAdaptiveGDRule(double u, double d)
            : base()
        {
            Contract.Requires(d > 0.0);
            Contract.Requires(u > 0.0);

            U = u;
            D = d;
        }

        public virtual double U { get; private set; }

        public virtual double D { get; private set; }

        [Required]
        [InitValue(DefMinStepSize)]
        [DefaultValue(DefMinStepSize)]
        [Category(PropertyCategories.Math)]
        public override double MaxStepSize
        {
            get { return base.MaxStepSize; }
            set
            {
                base.MaxStepSize = value;
                UpdateInitialStepSize();
            }
        }

        [Required]
        [InitValue(DefMaxStepSize)]
        [DefaultValue(DefMaxStepSize)]
        [Category(PropertyCategories.Math)]
        public override double MinStepSize
        {
            get { return base.MinStepSize; }
            set
            {
                base.MinStepSize = value;
                UpdateInitialStepSize();
            }
        }

        protected virtual void UpdateInitialStepSize()
        {
            //StepSize = StepSizeRange.PickRandomValue();
            StepSize = StepSizeRange.MinValue + StepSizeRange.Size / 2.0;
        }
    }
}
