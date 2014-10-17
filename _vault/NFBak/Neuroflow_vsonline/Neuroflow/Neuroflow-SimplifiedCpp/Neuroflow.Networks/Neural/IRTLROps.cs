using Neuroflow.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.Networks.Neural
{
    public interface IRTLROps
    {
        void Initialize(ActivationLayerForwardCompute forwardCompute);

        void ComputeForward(NeuralComputationContext context);
    }
}
