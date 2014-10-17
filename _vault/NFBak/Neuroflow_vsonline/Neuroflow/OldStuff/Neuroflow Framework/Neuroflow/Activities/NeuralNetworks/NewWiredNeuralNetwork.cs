using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core.NeuralNetworks.Architectures;

namespace Neuroflow.Activities.NeuralNetworks
{
    public sealed class NewWiredNeuralNetwork : NewArchitecturedNeuralNetwork<WiredNeuralArchitecture>
    {
        public NewWiredNeuralNetwork()
        {
            DisplayName = "Wired Neural Network";
        }
    }
}
