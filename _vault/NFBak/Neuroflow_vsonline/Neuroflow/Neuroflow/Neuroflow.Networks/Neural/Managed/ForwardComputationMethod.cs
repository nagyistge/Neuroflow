using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neuroflow.Networks.Neural
{
    public enum ForwardComputationMethod : byte
    {
        FeedForward,
        BPTT,
        RTLR
    }
}
