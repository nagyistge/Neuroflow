using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.NeuralNetworks
{
    public enum GradientComputationMethod
    {
        None,
        FeedForward,
        BPTT,
        RTLR
    }
}
