using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Neuroflow.Networks.Neural.Learning;
using System.ComponentModel.DataAnnotations;
using Neuroflow.Core.ComponentModel;
using System.ComponentModel;

namespace Neuroflow.Networks.Neural
{
    public sealed class ActivationLayer : LearningLayer
    {
        public ActivationLayer(
            [Required]
            [FreeDisplayName("Size")]
            [Category(PropertyCategories.Structure)]
            int size,
            [Required]
            [FreeDisplayName("Function")]
            [Category(PropertyCategories.Math)]
            ActivationFunction function,
            [FreeDisplayName("Learning Rules")]
            [Category(PropertyCategories.Algorithm)]
            params LearningRule[] learningRules)
            : base(size, learningRules)
        {
            Contract.Requires(size > 0);
            Contract.Requires(function != null);

            Function = function;
        }

        public ActivationFunction Function { get; private set; }

        public override bool IsBiased
        {
            get { return true; }
        }
    }
}