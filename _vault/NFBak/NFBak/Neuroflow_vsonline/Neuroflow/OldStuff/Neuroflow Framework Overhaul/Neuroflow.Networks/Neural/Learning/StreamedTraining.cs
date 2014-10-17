using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;
using Neuroflow.Core.ComponentModel;
using System.ComponentModel;

namespace Neuroflow.Networks.Neural.Learning
{
    public sealed class StreamedTraining : Training
    {
        public StreamedTraining(
            [Required]
            [Category(PropertyCategories.Structure)]
            [FreeDisplayName("Network")]
            NeuralNetwork network)
            : base(network, TrainingMode.Streamed)
        {
            Contract.Requires(network != null);
        }
    }
}
