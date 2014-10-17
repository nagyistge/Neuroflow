using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Neuroflow.ComponentModel;

namespace Neuroflow.Core.NeuralNetworks.Learning.Algorithms
{
    public abstract class MultimodeGradientRule : GradientRule
    {
        [Required]
        [InitValue(LearningMode.Stochastic)]
        [DefaultValue(LearningMode.Stochastic)]
        [Category(PropertyCategories.Behavior)]
        public LearningMode Mode { get; set; }

        protected internal override LearningMode GetMode()
        {
            return Mode;
        }
    }
}
