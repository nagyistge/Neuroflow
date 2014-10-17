using Neuroflow.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.Networks.Neural
{
    public interface IBPTTOps
    {
        void Initialize(ActivationLayerForwardCompute forwardCompute);

        void Initialize(ActivationLayerBackwardCompute backwardCompute);

        void ComputeForward(NeuralComputationContext context, int innerIterationIndex);

        void ComputeBackwardInternalStep(NeuralComputationContext context, int innerIterationIndex);

        void ComputeBackwardLastStep(NeuralComputationContext context, int innerIterationIndex);
    }
}
