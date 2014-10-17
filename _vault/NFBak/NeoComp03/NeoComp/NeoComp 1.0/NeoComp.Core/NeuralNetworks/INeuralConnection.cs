using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Computations;

namespace NeoComp.NeuralNetworks
{
    public interface INeuralConnection : IReset
    {
        double InputValue { get; }

        double OutputValue { get; }

        double Weight { get; set; }
    }
}
