using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.NeuralNetworks
{
    public sealed class RTLRLayerInfo
    {
        public int Index { get; internal set; }
        public IDeviceArray2 Weights { get; internal set; }
        public int Size { get; internal set; }
        public bool IsElementOfU { get; internal set; }
    }
}
