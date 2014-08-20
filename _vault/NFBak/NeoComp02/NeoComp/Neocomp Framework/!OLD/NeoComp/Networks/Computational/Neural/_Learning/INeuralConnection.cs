using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.Networks.Computational.Neural
{
    public interface INeuralConnection
    {
        double InputValue { get; }

        double OutputValue { get; }

        double Weight { get; set; }
    }
}
