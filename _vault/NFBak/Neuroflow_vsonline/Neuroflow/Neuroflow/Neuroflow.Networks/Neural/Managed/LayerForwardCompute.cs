using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Neuroflow.Core;
using System.Diagnostics;

namespace Neuroflow.Networks.Neural.Managed
{
    public abstract class LayerForwardCompute
    {
        protected LayerForwardCompute(ConnectedLayer connectedLayer)
        {
            Contract.Requires(connectedLayer != null);

            Init(connectedLayer);
        }

        #region Properties

        protected internal bool RunParallel { get; internal set; }

        public int ConnectedLayerIndex { get; private set; }

        public bool IsOutput { get; private set; }

        public InputValueAccess[] InputValueAccessItems { get; private set; }

        public IntRange OutputBuffer { get; private set; }

        public int? BiasValueIndex { get; private set; }

        public IntRange[] InnerItarationOutputValueStack { get; private set; }

        public ForwardComputationMethod Method { get; private set; }

        public UpperLayerInfo[] UpperNonInputLayerInfos { get; private set; }

        public IntRange[] PBiasBuffers { get; internal set; }

        public IntRange[][][] PWeightBuffers { get; internal set; }

        public IntRange[] PrevPBiasBuffers { get; internal set; }

        public IntRange[][][] PrevPWeightBuffers { get; internal set; }

        public IntRange? NetDerivBuffer { get; private set; }

        public int? BiasGradientValueIndex { get; private set; }

        public int? BiasGradientSumValueIndex { get; private set; }

        public IntRange[] GradientBuffers { get; private set; }

        public IntRange[] GradientSumBuffers { get; private set; }

        public IntRange? OutputErrorBuffer { get; private set; }

        #endregion

        #region Init

        private void Init(ConnectedLayer connectedLayer)
        {
            ConnectedLayerIndex = connectedLayer.Index;
            InitInputValueAccessItems(connectedLayer);
            OutputBuffer = connectedLayer.OutputBuffer;
            BiasValueIndex = connectedLayer.BiasValueIndex;
            InnerItarationOutputValueStack = connectedLayer.InnerItarationOutputValueStack;
            IsOutput = connectedLayer.IsOutput;
            if ((connectedLayer.StructuralElementFlags & NNStructuralElement.RTLRInformation) != 0)
            {
                Method = ForwardComputationMethod.RTLR;
                UpperNonInputLayerInfos = connectedLayer.UpperNonInputLayerInfos;
                PBiasBuffers = connectedLayer.PBiasBuffers;
                PWeightBuffers = connectedLayer.PWeightBuffers;
                PrevPBiasBuffers = connectedLayer.PrevPBiasBuffers;
                PrevPWeightBuffers = connectedLayer.PrevPWeightBuffers;
                NetDerivBuffer = connectedLayer.NetDerivBuffer;
                GradientBuffers = connectedLayer.GradientBuffers;
                GradientSumBuffers = connectedLayer.GradientSumBuffers;
                BiasGradientValueIndex = connectedLayer.BiasGradientValueIndex;
                BiasGradientSumValueIndex = connectedLayer.BiasGradientSumValueIndex;
            }
            else if (connectedLayer.InnerItarationOutputValueStack != null)
            {
                Debug.Assert(connectedLayer.InnerItarationInputValueStacks != null);

                Method = ForwardComputationMethod.BPTT;
            }
            else
            {
                Method = ForwardComputationMethod.FeedForward;
            }
        }

        private void InitInputValueAccessItems(ConnectedLayer connectedLayer)
        {
            int count = connectedLayer.WeightedInputBuffers.Length;
            var items = new InputValueAccess[count];

            for (int inputBufferIndex = 0; inputBufferIndex < count; inputBufferIndex++)
            {
                var weightedInputBuffer = connectedLayer.WeightedInputBuffers[inputBufferIndex];
                int wibValueBufferSize = weightedInputBuffer.ValueBuffer.Size;
                items[inputBufferIndex] =
                    new InputValueAccess(
                        weightedInputBuffer.ValueBuffer.Size,
                        weightedInputBuffer.ValueBuffer.MinValue,
                        weightedInputBuffer.WeightBuffer.MinValue,
                        connectedLayer.InnerItarationInputValueStacks != null ? connectedLayer.InnerItarationInputValueStacks[inputBufferIndex] : null);
            }

            InputValueAccessItems = items;
        }

        #endregion

        #region Backward Support

        protected internal virtual LayerBackwardCompute CreateBackwardCompute(ConnectedLayer connectedLayer)
        {
            Contract.Requires(connectedLayer != null);

            throw new InvalidOperationException("Backward computation is not supported.");
        } 

        #endregion

        #region Compute

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

        #endregion
    }
}
