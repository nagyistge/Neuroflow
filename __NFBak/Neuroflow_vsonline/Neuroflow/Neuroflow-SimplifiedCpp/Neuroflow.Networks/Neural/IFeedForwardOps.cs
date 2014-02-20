using Neuroflow.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.Networks.Neural
{
    public interface IFeedForwardOps
    {
        void Initialize(ActivationLayerForwardCompute forwardCompute);

        void Initialize(ActivationLayerBackwardCompute backwardCompute);

        void ComputeForward(NeuralComputationContext context);

        void ComputeBackward(NeuralComputationContext context);
    }
}
