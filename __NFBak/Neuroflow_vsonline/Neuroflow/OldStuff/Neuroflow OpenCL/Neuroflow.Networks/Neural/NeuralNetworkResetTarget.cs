using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neuroflow.Networks.Neural
{
    [Flags]
    public enum NeuralNetworkResetTarget : byte
    {
        Outputs = 1, 
        Errors = 2, 
        Gradients = 4, 
        GradientSums = 8,
        Ps = 16,
        Algorithms = 32,
        All = Outputs | Errors | Gradients | GradientSums | Ps | Algorithms
    }
}
