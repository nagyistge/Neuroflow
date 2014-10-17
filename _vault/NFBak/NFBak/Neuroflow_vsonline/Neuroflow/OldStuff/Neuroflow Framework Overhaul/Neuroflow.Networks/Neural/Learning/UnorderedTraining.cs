using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using Neuroflow.Core.ComponentModel;
using System.ComponentModel;
using System.Diagnostics.Contracts;

namespace Neuroflow.Networks.Neural.Learning
{
    public sealed class UnorderedTraining : Training
    {
        public UnorderedTraining(
            [Required]
            [Category(PropertyCategories.Structure)]
            [FreeDisplayName("Network")]
            NeuralNetwork network)
            : base(network, TrainingMode.Unordered)
        {
            Contract.Requires(network != null);
        }
    }
}
