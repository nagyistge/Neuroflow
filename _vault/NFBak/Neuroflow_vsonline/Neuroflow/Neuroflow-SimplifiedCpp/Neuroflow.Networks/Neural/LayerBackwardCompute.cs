﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Neuroflow.Core;
using System.Threading.Tasks;

namespace Neuroflow.Networks.Neural
{
    public abstract class LayerBackwardCompute
    {
        protected LayerBackwardCompute(LayerForwardCompute forwardCompute, ConnectedLayer connectedLayer)
        {
            Contract.Requires(forwardCompute != null);
            Contract.Requires(connectedLayer != null);
            Contract.Requires((connectedLayer.StructuralElementFlags & NNStructuralElement.BackwardImplementation) != 0 &&
                (connectedLayer.StructuralElementFlags & NNStructuralElement.GradientInformation) != 0);

            Init(forwardCompute, connectedLayer);
        }

        #region Properties

        public LayerForwardCompute ForwardCompute { get; private set; }

        public ErrorValueAccess[] LowerErrorValueAccessItems { get; private set; }

        public IntRange? ErrorBuffer { get; private set; }

        public int? BiasGradientValueIndex { get; private set; }

        public int? BiasGradientSumValueIndex { get; private set; }

        public IntRange[] GradientBuffers { get; private set; }

        public IntRange[] GradientSumBuffers { get; private set; }

        public IntRange? OutputErrorBuffer { get; private set; }

        #endregion

        #region Init

        private void Init(LayerForwardCompute forwardCompute, ConnectedLayer connectedLayer)
        {
            ForwardCompute = forwardCompute;

            InitLowerErrorValueAccessItems(connectedLayer);
            ErrorBuffer = connectedLayer.ErrorBuffer;
            GradientBuffers = connectedLayer.GradientBuffers;
            GradientSumBuffers = connectedLayer.GradientSumBuffers;
            BiasGradientValueIndex = connectedLayer.BiasGradientValueIndex;
            BiasGradientSumValueIndex = connectedLayer.BiasGradientSumValueIndex;
            OutputErrorBuffer = connectedLayer.OutputErrorBuffer;
        }

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

        internal abstract void Compute(NeuralComputationContext context, BackprogrationMode mode, int? innerIterationIndex);

        internal abstract void SetError(NeuralComputationContext context, IntRange errorBuffer);

        internal void Reset(NeuralComputationContext context, NeuralNetworkResetTarget target)
        {
            if ((target & NeuralNetworkResetTarget.Errors) == NeuralNetworkResetTarget.Errors)
            {
                ForwardCompute.BufferOps.Zero(context, ErrorBuffer.Value);
            }

            if ((target & NeuralNetworkResetTarget.Gradients) == NeuralNetworkResetTarget.Gradients)
            {
                foreach (var gradBuff in GradientBuffers)
                {
                    ForwardCompute.BufferOps.Zero(context, gradBuff);
                }
            }

            if ((target & NeuralNetworkResetTarget.GradientSums) == NeuralNetworkResetTarget.GradientSums)
            {
                foreach (var gradSumBuff in GradientSumBuffers)
                {
                    ForwardCompute.BufferOps.Zero(context, gradSumBuff);
                }
            }
        }
    }
}
