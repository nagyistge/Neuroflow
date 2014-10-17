using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Neuroflow.Core;

namespace Neuroflow.Networks.Neural
{
    public abstract class LayerBackwardComputeBase
    {
        #region Construct

        protected LayerBackwardComputeBase(LayerForwardComputeBase forwardCompute, ConnectedLayer connectedLayer)
        {
            Contract.Requires(forwardCompute != null);
            Contract.Requires(connectedLayer != null);
            Contract.Requires((connectedLayer.StructuralElementFlags & NNStructuralElement.BackwardImplementation) != 0 &&
                (connectedLayer.StructuralElementFlags & NNStructuralElement.GradientInformation) != 0);

            ForwardCompute = forwardCompute;

            InitLowerErrorValueAccessItems(connectedLayer);
            ErrorBuffer = connectedLayer.ErrorBuffer;
            GradientBuffers = connectedLayer.GradientBuffers;
            GradientSumBuffers = connectedLayer.GradientSumBuffers;
            BiasGradientValueIndex = connectedLayer.BiasGradientValueIndex;
            BiasGradientSumValueIndex = connectedLayer.BiasGradientSumValueIndex;
            OutputErrorBuffer = connectedLayer.OutputErrorBuffer;
        }

        #endregion

        #region Properties

        public LayerForwardComputeBase ForwardCompute { get; private set; }

        public ErrorValueAccess[] LowerErrorValueAccessItems { get; private set; }

        public IntRange? ErrorBuffer { get; private set; }

        public int? BiasGradientValueIndex { get; private set; }

        public int? BiasGradientSumValueIndex { get; private set; }

        public IntRange[] GradientBuffers { get; private set; }

        public IntRange[] GradientSumBuffers { get; private set; }

        public IntRange? OutputErrorBuffer { get; private set; } 

        #endregion

        #region Init

        private void InitLowerErrorValueAccessItems(ConnectedLayer connectedLayer)
        {
            if (connectedLayer.WeightedOutputErrorBuffers == null) return;
            
            var items = new LinkedList<ErrorValueAccess>();

            for (int outputErrorBufferIndex = 0; outputErrorBufferIndex < connectedLayer.WeightedOutputErrorBuffers.Length; outputErrorBufferIndex++)
            {
                var weightedOutputErrorBuffer = connectedLayer.WeightedOutputErrorBuffers[outputErrorBufferIndex];
                int wibValueBufferSize = weightedOutputErrorBuffer.ValueBuffer.Size;
                items.AddLast(
                    new ErrorValueAccess(
                        weightedOutputErrorBuffer.ValueBuffer.Size,
                        weightedOutputErrorBuffer.ValueBuffer.MinValue,
                        weightedOutputErrorBuffer.WeightBuffer.MinValue));
            }

            LowerErrorValueAccessItems = items.ToArray();
        } 

        #endregion
    }
}
