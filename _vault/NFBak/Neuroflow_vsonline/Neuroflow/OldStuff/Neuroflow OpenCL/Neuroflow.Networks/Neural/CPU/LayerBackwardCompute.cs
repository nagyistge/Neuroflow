using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Neuroflow.Core;
using System.Threading.Tasks;

namespace Neuroflow.Networks.Neural.CPU
{
    public abstract class LayerBackwardCompute : LayerBackwardComputeBase
    {
        protected LayerBackwardCompute(LayerForwardCompute forwardCompute, ConnectedLayer connectedLayer)
            : base(forwardCompute, connectedLayer)
        {
            Contract.Requires(forwardCompute != null);
            Contract.Requires(connectedLayer != null);
            Contract.Requires((connectedLayer.StructuralElementFlags & NNStructuralElement.BackwardImplementation) != 0 &&
                (connectedLayer.StructuralElementFlags & NNStructuralElement.GradientInformation) != 0);

            RunParallel = forwardCompute.RunParallel;
        }

        protected bool RunParallel { get; private set; }

        unsafe protected internal abstract void SetErrors(float* valueBuffer, float[] errors);

        unsafe protected internal abstract void Compute(float* valueBuffer, BackprogrationMode mode, int? innerIterationIndex);

        unsafe internal void Reset(float[] valueBuffer, NeuralNetworkResetTarget target)
        {
            if ((target & NeuralNetworkResetTarget.Errors) == NeuralNetworkResetTarget.Errors)
            {
                ValueBuffer.Zero(valueBuffer, ErrorBuffer.Value);
            }

            if ((target & NeuralNetworkResetTarget.Gradients) == NeuralNetworkResetTarget.Gradients)
            {
                foreach (var gradBuff in GradientBuffers)
                {
                    ValueBuffer.Zero(valueBuffer, gradBuff);
                }
            }

            if ((target & NeuralNetworkResetTarget.GradientSums) == NeuralNetworkResetTarget.GradientSums)
            {
                foreach (var gradSumBuff in GradientSumBuffers)
                {
                    ValueBuffer.Zero(valueBuffer, gradSumBuff);
                }
            }
        }
    }
}
