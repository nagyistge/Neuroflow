using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.NeuralNetworks
{
    public sealed class RTLRComputationData
    {
        public DeviceArrayFactory Inputs { get; internal set; }
        public IDeviceArray2 Gradients { get; internal set; }
        public IDeviceArray2 GradientSums { get; internal set; }
        public IDeviceArray BiasGradients { get; internal set; }
        public IDeviceArray BiasGradientSums { get; internal set; }
        public int ILayerIndex { get; internal set; }
        public int IValueIndex { get; internal set; }
        public int JLayerIndex { get; internal set; }
        public int JValueIndex { get; internal set; }
    }
}
