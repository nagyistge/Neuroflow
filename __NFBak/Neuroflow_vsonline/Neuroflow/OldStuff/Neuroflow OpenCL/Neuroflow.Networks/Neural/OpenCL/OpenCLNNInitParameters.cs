using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Neuroflow.Core.ComponentModel;

namespace Neuroflow.Networks.Neural.OpenCL
{
    public sealed class OpenCLNNInitParameters : NNInitParameters
    {
        public override Type NeuralNetworkType
        {
            get { return typeof(OpenCLNeuralNetwork); }
        }
        
        [DisplayName("Platform Name")]
        [Category(PropertyCategories.OpenCL)]
        public string PlatformName { get; set; }

        [DisplayName("Device Name")]
        [Category(PropertyCategories.OpenCL)]
        public string DeviceName { get; set; }
    }
}
