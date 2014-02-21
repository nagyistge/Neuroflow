using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Computations;

namespace NeoComp.Networks.Computational.Neural
{
    public interface INeuralConnection : IReset
    {
        double InputValue { get; }

        double OutputValue { get; }

        double Weight { get; set; }
    }
}
