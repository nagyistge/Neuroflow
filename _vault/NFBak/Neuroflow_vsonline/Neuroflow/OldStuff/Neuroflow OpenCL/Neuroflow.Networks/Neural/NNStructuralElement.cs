using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neuroflow.Networks.Neural
{
    [Flags]
    public enum NNStructuralElement
    {
        None = 0,
        BackwardImplementation = 1,
        GradientInformation = 2,
        RTLRInformation = 4
    }
}
