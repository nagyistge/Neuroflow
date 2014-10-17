using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Diagnostics;
using Neuroflow.Core;

namespace Neuroflow.Networks.Neural.CPU
{
    public class ActivationLayerForwardCompute : LayerForwardCompute
    {
        #region Constructor

        public ActivationLayerForwardCompute(ConnectedLayer connectedLayer)
            : base(connectedLayer)
        {
            Contract.Requires(connectedLayer != null);
            Contract.Requires(connectedLayer.Layer is ActivationLayer);

            Function = ((ActivationLayer)connectedLayer.Layer).Function;
        } 

        #endregion

        #region Properties

        public ActivationFunction Function { get; private set; } 

        #endregion

        #region BW Factory

        protected internal override LayerBackwardCompute CreateBackwardCompute(ConnectedLayer connectedLayer)
        {
            return new ActivationLayerBackwardCompute(this, connectedLayer);
        } 

        #endregion

        #region Computing

        #region Compute

        protected internal override unsafe void Compute(float* valueBuffer, bool collectTrainingData, int? innerIterationIndex)
        {
            if (collectTrainingData)
            {
                switch (Method)
                {
                    case ForwardComputationMethod.FeedForward:
                        Compute_FeedForward(valueBuffer);
                        break;
                    case ForwardComputationMethod.BPTT:
                        Compute_BPTT(valueBuffer, innerIterationIndex.Value);
                        break;
                    case ForwardComputationMethod.RTLR:
                        Compute_RTLR(valueBuffer);
                        break;
                }
            }
            else
            {
                Compute_FeedForward(valueBuffer);
            }
        }

        unsafe private void Compute_FeedForward(float* valueBuffer)
        {
            DataParallel.Do(OutputBuffer.Size, RunParallel, (ctx) =>
            {
                for (int outputIndex = ctx.WorkItemRange.MinValue; outputIndex <= ctx.WorkItemRange.MaxValue; outputIndex++)
                {
                    Compute_FeedForward(valueBuffer, outputIndex);
                }
            });
        }

        unsafe private void Compute_BPTT(float* valueBuffer, int innerIterationIndex)
        {
            DataParallel.Do(OutputBuffer.Size, RunParallel, (ctx) =>
            {
                for (int outputIndex = ctx.WorkItemRange.MinValue; outputIndex <= ctx.WorkItemRange.MaxValue; outputIndex++)
                {
                    Compute_BPTT(valueBuffer, outputIndex, innerIterationIndex);
                }
            });
        }

        unsafe private void Compute_RTLR(float* valueBuffer)
        {
            DataParallel.Do(OutputBuffer.Size, RunParallel, (ctx) =>
            {
                for (int outputIndex = ctx.WorkItemRange.MinValue; outputIndex <= ctx.WorkItemRange.MaxValue; outputIndex++)
                {
                    Compute_RTLR(valueBuffer, outputIndex);
                }
            });
        }

        #endregion

        #region Feed Forward

        private unsafe void Compute_FeedForward(float* valueBuffer, int outputIndex)
        {
            // Calculate sum:
            float sum = BiasValueIndex != null ? valueBuffer[BiasValueIndex.Value] : 0.0f;

            int outputSize = OutputBuffer.Size;
            foreach (var accessItem in InputValueAccessItems)
            {
                for (int inputIndex = 0; inputIndex < accessItem.InputSize; inputIndex++)
                {
                    int inputValueIndex = accessItem.InputBufferBeginIndex + inputIndex;

                    sum += valueBuffer[inputValueIndex] * valueBuffer[accessItem.WeightBufferBeginIndex + WeightAccessor.GetWeightValueIndex(inputIndex, outputIndex, outputSize)];
                }
            }

            // Set output derivate
            valueBuffer[OutputBuffer.MinValue + outputIndex] = Function.Calculate(sum);
        }

        #endregion

        #region BPTT

        private unsafe void Compute_BPTT(float* valueBuffer, int outputIndex, int innerIterationIndex)
        {
            // Calculate sum:
            float sum = BiasValueIndex != null ? valueBuffer[BiasValueIndex.Value] : 0.0f;

            int outputSize = OutputBuffer.Size;
            foreach (var accessItem in InputValueAccessItems)
            {
                for (int inputIndex = 0; inputIndex < accessItem.InputSize; inputIndex++)
                {
                    int inputValueIndex = accessItem.InputBufferBeginIndex + inputIndex;

                    sum += valueBuffer[inputValueIndex] * valueBuffer[accessItem.WeightBufferBeginIndex + WeightAccessor.GetWeightValueIndex(inputIndex, outputIndex, outputSize)];

                    // With tracking, put inputs:
                    Debug.Assert(accessItem.InnerItarationInputValueStack != null);
                    Debug.Assert(accessItem.InnerItarationInputValueStack.Length > inputIndex);

                    var range = accessItem.InnerItarationInputValueStack[inputIndex];
                    valueBuffer[range.MinValue + innerIterationIndex] = valueBuffer[inputValueIndex];
                }
            }

            // Set output derivate
            // With tracking
            Debug.Assert(InnerItarationOutputValueStack != null);
            Debug.Assert(InnerItarationOutputValueStack.Length > outputIndex);

            var stackBuffer = InnerItarationOutputValueStack[outputIndex];

            Debug.Assert(stackBuffer.Size > innerIterationIndex);

            valueBuffer[stackBuffer.MinValue + innerIterationIndex] = Function.CalculateDerivate(valueBuffer[OutputBuffer.MinValue + outputIndex] = Function.Calculate(sum));
        } 

        #endregion

        #region RTLR

        private unsafe void Compute_RTLR(float* valueBuffer, int outputIndex)
        {
            // Calculate sum:
            float sum = BiasValueIndex != null ? valueBuffer[BiasValueIndex.Value] : 0.0f;

            int outputSize = OutputBuffer.Size;
            foreach (var accessItem in InputValueAccessItems)
            {
                for (int inputIndex = 0; inputIndex < accessItem.InputSize; inputIndex++)
                {
                    int inputValueIndex = accessItem.InputBufferBeginIndex + inputIndex;

                    sum += valueBuffer[inputValueIndex] * valueBuffer[accessItem.WeightBufferBeginIndex + WeightAccessor.GetWeightValueIndex(inputIndex, outputIndex, outputSize)];
                }
            }

            // Store Net Value:
            valueBuffer[NetDerivBuffer.Value.MinValue + outputIndex] = Function.CalculateDerivate(sum);

            // Set output derivate
            valueBuffer[OutputBuffer.MinValue + outputIndex] = Function.Calculate(sum);
        } 

        #endregion

        #endregion
    }
}
