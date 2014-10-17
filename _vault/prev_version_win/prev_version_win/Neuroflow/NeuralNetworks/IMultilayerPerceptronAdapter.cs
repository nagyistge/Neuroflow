using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.NeuralNetworks
{
    public interface IMultilayerPerceptronAdapter
    {
        IDeviceArrayManagement DeviceArrayManagement { get; }

        VectorUtils VectorUtils { get; }

        IComputeActivation ComputeActivation { get; }

        ILearningAlgoFactory LearningAlgoFactory { get; }
    }
}
