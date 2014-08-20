using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Networks.Neural;

namespace Neuroflow.Activities.Networks.Neural
{
    public sealed class NewStandardMultilayerNeuralNetwork : NewArchitecturedNeuralNetwork<StandardMultilayerArchitecture>
    {
        public NewStandardMultilayerNeuralNetwork()
        {
            DisplayName = "Standard Multilayer NN";
        }
    }
}
