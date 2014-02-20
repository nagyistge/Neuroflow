using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.NeuralNetworks.ActivationFunctions
{
    public interface IActivationFunction
    {
        double Function(double value);

        double Derivate(double value);

        double Alpha { get; set; }
    }
}
