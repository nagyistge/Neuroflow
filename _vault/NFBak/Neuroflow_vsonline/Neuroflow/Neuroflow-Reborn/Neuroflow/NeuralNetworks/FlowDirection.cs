using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.NeuralNetworks
{
    [Flags]
    public enum FlowDirection 
    { 
        None = 0,
        OneWay = 1 << 0, 
        TwoWay = 1 << 2, 
        OneWayToSource = 1 << 3,
        All = OneWay | OneWayToSource | TwoWay
    }
}
