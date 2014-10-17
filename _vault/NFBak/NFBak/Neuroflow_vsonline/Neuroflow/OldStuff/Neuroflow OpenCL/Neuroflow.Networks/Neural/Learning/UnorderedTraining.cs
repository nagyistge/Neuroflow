using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Neuroflow.Networks.Neural.Learning
{
    public sealed class UnorderedTraining : Training
    {
        public UnorderedTraining(NeuralNetwork network)
            : base(network, TrainingMode.Unordered)
        {
            Contract.Requires(network != null);
        }
    }
}
