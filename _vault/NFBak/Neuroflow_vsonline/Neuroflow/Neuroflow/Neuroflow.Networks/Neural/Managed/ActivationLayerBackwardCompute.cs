using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Neuroflow.Core;
using System.Diagnostics;

namespace Neuroflow.Networks.Neural.Managed
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
        } 

        #endregion

        #region Props

        protected ActivationFunction Function { get; private set; }

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

        #region Compute

        protected internal override unsafe void Compute(float* valueBuffer, BackprogrationMode mode, int? innerIterationIndex)
        {
            Debug.Assert((mode == BackprogrationMode.FeedForward && innerIterationIndex == null) || (mode != BackprogrationMode.FeedForward && innerIterationIndex != null));
            Debug.Assert(ErrorBuffer != null && ErrorBuffer.Value.Size == ForwardCompute.OutputBuffer.Size);
            Debug.Assert(LowerErrorValueAccessItems != null);

            switch (mode)
            {
                case BackprogrationMode.FeedForward:
                    Compute_FeedForward(valueBuffer);
                    break;
                case BackprogrationMode.BPTTInternalStep:
                    Compute_BPTTInternalStep(valueBuffer, innerIterationIndex.Value);
                    break;
                case BackprogrationMode.BPTTLastStep:
                    Compute_BPTTLastStep(valueBuffer, innerIterationIndex.Value);
                    break;
            }
        }

        unsafe private void Compute_FeedForward(float* valueBuffer)
        {
            for (int outputIndex = 0; outputIndex < ForwardCompute.OutputBuffer.Size; outputIndex++)
            {
                SetGradients_FeedForward(valueBuffer, outputIndex, ComputeError_FF(valueBuffer, outputIndex));
            }
        }

        unsafe private void Compute_BPTTInternalStep(float* valueBuffer, int innerIterationIndex)
        {
            for (int outputIndex = 0; outputIndex < ForwardCompute.OutputBuffer.Size; outputIndex++)
            {
                SetGradients_BPTInt(valueBuffer, outputIndex, ComputeError_BPTT(valueBuffer, outputIndex, innerIterationIndex), innerIterationIndex);
            }
        }

        unsafe private void Compute_BPTTLastStep(float* valueBuffer, int innerIterationIndex)
        {
            for (int outputIndex = 0; outputIndex < ForwardCompute.OutputBuffer.Size; outputIndex++)
            {
                SetGradients_BPTTLast(valueBuffer, outputIndex, ComputeError_BPTT(valueBuffer, outputIndex, innerIterationIndex), innerIterationIndex);
            }
        }

        #endregion

        #region Error

        private unsafe float ComputeError_FF(float* valueBuffer, int outputIndex)
        {
            float sum = ComputeWeightedErrorSum(valueBuffer, outputIndex);

            // Compute error:
            return valueBuffer[ErrorBuffer.Value.MinValue + outputIndex] =
                (sum * Function.CalculateDerivate(valueBuffer[ForwardCompute.OutputBuffer.MinValue + outputIndex]));
        }

        private unsafe float ComputeError_BPTT(float* valueBuffer, int outputIndex, int innerIterationIndex)
        {
            float sum = ComputeWeightedErrorSum(valueBuffer, outputIndex);

            // Compute error:
            var innerItarationOutputValueStack = ForwardCompute.InnerItarationOutputValueStack;

            // Recurrent, derivate is in the stack
            Debug.Assert(innerItarationOutputValueStack != null);
            Debug.Assert(innerItarationOutputValueStack.Length > outputIndex);

            var derivBuffer = innerItarationOutputValueStack[outputIndex];

            Debug.Assert(derivBuffer.Size > innerIterationIndex);

            return valueBuffer[ErrorBuffer.Value.MinValue + outputIndex] =
                (sum * valueBuffer[derivBuffer.MinValue + innerIterationIndex]);
        }

        unsafe private float ComputeWeightedErrorSum(float* valueBuffer, int outputIndex)
        {
            // Compute weighted error sum:
            float sum = 0.0f;

            if (OutputErrorBuffer.HasValue) // This is the output layer
            {
                // External errors are set:
                int errorValueIndex = OutputErrorBuffer.Value.MinValue + outputIndex;
                sum = valueBuffer[errorValueIndex];
            }

            foreach (var lowerErrorAccessItem in LowerErrorValueAccessItems)
            {
                for (int outputErrorIndex = 0; outputErrorIndex < lowerErrorAccessItem.ErrorSize; outputErrorIndex++)
                {
                    int outputErrorValueIndex = lowerErrorAccessItem.ErrorBufferBeginIndex + outputErrorIndex;

                    sum += valueBuffer[outputErrorValueIndex] * valueBuffer[lowerErrorAccessItem.WeightBufferBeginIndex + WeightAccessor.GetWeightValueIndex(outputIndex, outputErrorIndex, lowerErrorAccessItem.ErrorSize)];
                }
            }
            return sum;
        } 

        #endregion

        #region Gradient

        private unsafe void SetGradients_FeedForward(float* valueBuffer, int outputIndex, float error)
        {
            // Bias:
            valueBuffer[BiasGradientValueIndex.Value] = error;
            valueBuffer[BiasGradientSumValueIndex.Value] += error;

            // Inputs:
            int outputSize = ForwardCompute.OutputBuffer.Size;
            int layerIndex = 0;
            fixed (IntRange* gradientBuffers = GradientBuffers)
            fixed (IntRange* gradientSumBuffers = GradientSumBuffers)
            {
                var gradientBuff = gradientBuffers[layerIndex];
                var gradientSumBuff = gradientSumBuffers[layerIndex];

                foreach (var inputAccess in ForwardCompute.InputValueAccessItems)
                {
                    for (int inputIndex = 0; inputIndex < inputAccess.InputSize; inputIndex++)
                    {
                        int inputValueIndex = inputAccess.InputBufferBeginIndex + inputIndex;
                        float gradient = error * valueBuffer[inputValueIndex];

                        int wvIndex = WeightAccessor.GetWeightValueIndex(inputIndex, outputIndex, outputSize);
                        valueBuffer[gradientBuff.MinValue + wvIndex] = gradient;
                        valueBuffer[gradientSumBuff.MinValue + wvIndex] += gradient;
                    }

                    layerIndex++;
                }
            }
        }

        private unsafe void SetGradients_BPTInt(float* valueBuffer, int outputIndex, float error, int innerItarationIndex)
        {
            // Bias:
            valueBuffer[BiasGradientValueIndex.Value] += error;

            // Inputs:
            int outputSize = ForwardCompute.OutputBuffer.Size;
            int layerIndex = 0;
            fixed (IntRange* gradientBuffers = GradientBuffers)
            {
                var gradientBuff = gradientBuffers[layerIndex];

                foreach (var inputAccess in ForwardCompute.InputValueAccessItems)
                {
                    fixed (IntRange* inputValueStacks = inputAccess.InnerItarationInputValueStack)
                    {
                        for (int inputIndex = 0; inputIndex < inputAccess.InputSize; inputIndex++)
                        {
                            var inputValueStack = inputValueStacks[inputIndex];
                            int inputValueIndex = inputValueStack.MinValue + innerItarationIndex;

                            Debug.Assert(inputValueIndex <= inputValueStack.MaxValue);

                            float gradient = error * valueBuffer[inputValueIndex];

                            valueBuffer[gradientBuff.MinValue + WeightAccessor.GetWeightValueIndex(inputIndex, outputIndex, outputSize)] += gradient;
                        }
                    }

                    layerIndex++;
                }
            }
        }

        private unsafe void SetGradients_BPTTLast(float* valueBuffer, int outputIndex, float error, int innerItarationIndex)
        {
            // Bias:
            valueBuffer[BiasGradientValueIndex.Value] += error;
            valueBuffer[BiasGradientSumValueIndex.Value] += valueBuffer[BiasGradientValueIndex.Value];

            // Inputs:
            int outputSize = ForwardCompute.OutputBuffer.Size;
            int layerIndex = 0;
            fixed (IntRange* gradientBuffers = GradientBuffers)
            fixed (IntRange* gradientSumBuffers = GradientSumBuffers)
            {
                var gradientBuff = gradientBuffers[layerIndex];
                var gradientSumBuff = gradientSumBuffers[layerIndex];

                foreach (var inputAccess in ForwardCompute.InputValueAccessItems)
                {
                    fixed (IntRange* inputValueStacks = inputAccess.InnerItarationInputValueStack)
                    {
                        for (int inputIndex = 0; inputIndex < inputAccess.InputSize; inputIndex++)
                        {
                            var inputValueStack = inputValueStacks[inputIndex];
                            int inputValueIndex = inputValueStack.MinValue + innerItarationIndex;

                            Debug.Assert(inputValueIndex <= inputValueStack.MaxValue);

                            float gradient = error * valueBuffer[inputValueIndex];

                            int wvIndex = WeightAccessor.GetWeightValueIndex(inputIndex, outputIndex, outputSize);
                            valueBuffer[gradientSumBuff.MinValue + wvIndex] += (valueBuffer[gradientBuff.MinValue + wvIndex] += gradient);
                        }
                    }

                    layerIndex++;
                }
            }
        }          

        #endregion

        #endregion
    }
}
