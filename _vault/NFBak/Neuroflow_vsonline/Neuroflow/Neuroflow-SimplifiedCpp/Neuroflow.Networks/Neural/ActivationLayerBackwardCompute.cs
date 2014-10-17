using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Neuroflow.Core;
using System.Diagnostics;

namespace Neuroflow.Networks.Neural
{
    public class ActivationLayerBackwardCompute : LayerBackwardCompute
    {
        #region Construct

        public ActivationLayerBackwardCompute(ActivationLayerForwardCompute forwardCompute, ConnectedLayer connectedLayer)
            : base(forwardCompute, connectedLayer)
        {
            Contract.Requires(forwardCompute != null);
            Contract.Requires(connectedLayer != null);
            Contract.Requires((connectedLayer.StructuralElementFlags & NNStructuralElement.BackwardImplementation) != 0 &&
                (connectedLayer.StructuralElementFlags & NNStructuralElement.GradientInformation) != 0);

            Function = forwardCompute.Function;

            forwardCompute.FeedForwardOps.Initialize(this);
            forwardCompute.BPTTOps.Initialize(this);
        } 

        #endregion

        #region Props

        public ActivationFunction Function { get; private set; }

        #endregion

        #region Set Error on Output

        protected internal override unsafe void SetErrors(float* valueBuffer, IntRange errors)
        {
            Debug.Assert(OutputErrorBuffer.HasValue);
            Debug.Assert(OutputErrorBuffer.Value.Size == errors.Size);

            var eb = OutputErrorBuffer.Value;
            int ebSize = eb.Size;
            for (int i = 0; i < ebSize; i++)
            {
                valueBuffer[eb.MinValue + i] = valueBuffer[errors.MinValue + i];
            }
        } 

        #endregion

        #region Computing

        internal override void Compute(NeuralComputationContext context, BackprogrationMode mode, int? innerIterationIndex)
        {
            Debug.Assert((mode == BackprogrationMode.FeedForward && innerIterationIndex == null) || (mode != BackprogrationMode.FeedForward && innerIterationIndex != null));
            Debug.Assert(ErrorBuffer != null && ErrorBuffer.Value.Size == ForwardCompute.OutputBuffer.Size);
            Debug.Assert(LowerErrorValueAccessItems != null);

            switch (mode)
            {
                case BackprogrationMode.FeedForward:
                    ((ActivationLayerForwardCompute)ForwardCompute).FeedForwardOps.ComputeBackward(context);
                    break;
                case BackprogrationMode.BPTTInternalStep:
                    ((ActivationLayerForwardCompute)ForwardCompute).BPTTOps.ComputeBackwardInternalStep(context, innerIterationIndex.Value);
                    break;
                case BackprogrationMode.BPTTLastStep:
                    ((ActivationLayerForwardCompute)ForwardCompute).BPTTOps.ComputeBackwardLastStep(context, innerIterationIndex.Value);
                    break;
            }
        }

        #endregion
    }
}
