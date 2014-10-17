using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neuroflow.Core.NeuralNetworks.ActivationFunctions
{
    public interface IActivationFunction
    {
        double Function(double value);

        double Derivate(double value);

        double Alpha { get; set; }
    }
}
