using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.NeuralNetworks
{
    public sealed class ManagedMultiplayerPerceptronAdapter : IMultilayerPerceptronAdapter
    {
        ManagedComputeActivation managedComputeNNActivation;

        ManagedDeviceArrayManagement managedDeviceArrayManagement;

        ManagedLearningAlgoFactory managedLearningAlgoFactory;

        ManagedVectorUtils managedVectorUtils;

        public IDeviceArrayManagement DeviceArrayManagement
        {
            get { return managedDeviceArrayManagement ?? (managedDeviceArrayManagement = new ManagedDeviceArrayManagement()); }
        }

        public VectorUtils VectorUtils
        {
            get { return managedVectorUtils ?? (managedVectorUtils = new ManagedVectorUtils()); }
        }

        public IComputeActivation ComputeActivation
        {
            get { return managedComputeNNActivation ?? (managedComputeNNActivation = new ManagedComputeActivation()); }
        }

        public ILearningAlgoFactory LearningAlgoFactory
        {
            get { return managedLearningAlgoFactory ?? (managedLearningAlgoFactory = new ManagedLearningAlgoFactory()); }
        }
    }
}
