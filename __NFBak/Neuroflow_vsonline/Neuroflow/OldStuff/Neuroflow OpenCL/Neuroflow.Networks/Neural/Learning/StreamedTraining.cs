using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Neuroflow.Networks.Neural.Learning
{
    public sealed class StreamedTraining : Training
    {
        public StreamedTraining(NeuralNetwork network)
            : base(network, TrainingMode.Streamed)
        {
            Contract.Requires(network != null);
        }
    }
}
