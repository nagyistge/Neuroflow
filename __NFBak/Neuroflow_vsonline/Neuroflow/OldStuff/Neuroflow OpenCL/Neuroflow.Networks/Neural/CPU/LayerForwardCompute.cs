using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Neuroflow.Core;

namespace Neuroflow.Networks.Neural.CPU
{
    public abstract class LayerForwardCompute : LayerForwardComputeBase
    {
        protected LayerForwardCompute(ConnectedLayer connectedLayer)
            : base(connectedLayer)
        {
            Contract.Requires(connectedLayer != null);
        }

        protected internal bool RunParallel { get; internal set; }



        protected internal virtual LayerBackwardCompute CreateBackwardCompute(ConnectedLayer connectedLayer)
        {
            Contract.Requires(connectedLayer != null);

            throw new InvalidOperationException("Backward computation is not supported.");
        }

        unsafe protected internal abstract void Compute(float* valueBuffer, bool collectTrainingData, int? innerIterationIndex);

        unsafe internal void Reset(float[] valueBuffer, NeuralNetworkResetTarget target)
        {
            if ((target & NeuralNetworkResetTarget.Outputs) != 0)
            {
                ValueBuffer.Zero(valueBuffer, OutputBuffer);
            }
            else if ((target & NeuralNetworkResetTarget.Ps) != 0)
            {
                if (Method == ForwardComputationMethod.RTLR)
                {
                    foreach (var range in PBiasBuffers)
                    {
                        ValueBuffer.Zero(valueBuffer, range);
                    }

                    foreach (var lvl1 in PWeightBuffers)
                    {
                        foreach (var lvl2 in lvl1)
                        {
                            foreach (var range in lvl2)
                            {
                                ValueBuffer.Zero(valueBuffer, range);
                            }
                        }
                    }
                }
            }
        }
    }
}
