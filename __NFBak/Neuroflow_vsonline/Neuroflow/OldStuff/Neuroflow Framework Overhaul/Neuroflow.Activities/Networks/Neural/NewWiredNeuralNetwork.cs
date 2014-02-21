using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Networks.Neural;

namespace Neuroflow.Activities.Networks.Neural
{
    public sealed class NewWiredNeuralNetwork : NewArchitecturedNeuralNetwork<WiredArchitecture>
    {
        public NewWiredNeuralNetwork()
        {
            DisplayName = "Wired Neural Network";
        }
    }
}
