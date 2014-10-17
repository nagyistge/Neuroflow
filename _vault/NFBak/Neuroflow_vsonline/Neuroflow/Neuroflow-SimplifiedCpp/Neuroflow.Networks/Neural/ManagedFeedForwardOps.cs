using Neuroflow.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.Networks.Neural
{
    public sealed class ManagedFeedForwardOps : IFeedForwardOps
    {
        ActivationLayerForwardCompute forwardCompute;

        ActivationLayerBackwardCompute backwardCompute;

        public void Initialize(ActivationLayerForwardCompute forwardCompute)
        {
            this.forwardCompute = forwardCompute;
        }

        public void Initialize(ActivationLayerBackwardCompute backwardCompute)
        {
            this.backwardCompute = backwardCompute;
        }

        unsafe public void ComputeForward(NeuralComputationContext context)
        {
            var valueBuffer = ManagedBufferOps.GetValueBuff(context);
            fixed (float* pValueBuffer = valueBuffer)
            {
                for (int outputIndex = 0; outputIndex < forwardCompute.OutputBuffer.Size; outputIndex++)
                {
                    // Calculate sum:
                    float sum = forwardCompute.BiasValueIndex != null ? pValueBuffer[forwardCompute.BiasValueIndex.Value] : 0.0f;

                    int outputSize = forwardCompute.OutputBuffer.Size;
                    foreach (var accessItem in forwardCompute.InputValueAccessItems)
                    {
                        for (int inputIndex = 0; inputIndex < accessItem.InputSize; inputIndex++)
                        {
                            int inputValueIndex = accessItem.InputBufferBeginIndex + inputIndex;

                            sum += pValueBuffer[inputValueIndex] * pValueBuffer[accessItem.WeightBufferBeginIndex + WeightAccessor.GetWeightValueIndex(inputIndex, outputIndex, outputSize)];
                        }
                    }

                    // Set output derivate
                    pValueBuffer[forwardCompute.OutputBuffer.MinValue + outputIndex] = forwardCompute.Function.Calculate(sum);
                }
            }
        }

        unsafe public void ComputeBackward(NeuralComputationContext context)
        {
            var valueBuffer = ManagedBufferOps.GetValueBuff(context);
            fixed (float* pValueBuffer = valueBuffer)
            {
                for (int outputIndex = 0; outputIndex < backwardCompute.ForwardCompute.OutputBuffer.Size; outputIndex++)
                {
                    SetGradients(backwardCompute, pValueBuffer, outputIndex, ComputeError(backwardCompute, pValueBuffer, outputIndex));
                }
            }
        }

        private static unsafe float ComputeError(ActivationLayerBackwardCompute backwardCompute, float* valueBuffer, int outputIndex)
        {
            float sum = ComputeWeightedErrorSum(backwardCompute, valueBuffer, outputIndex);

            // Compute error:
            return valueBuffer[backwardCompute.ErrorBuffer.Value.MinValue + outputIndex] =
                (sum * backwardCompute.Function.CalculateDerivate(valueBuffer[backwardCompute.ForwardCompute.OutputBuffer.MinValue + outputIndex]));
        }

        unsafe static internal float ComputeWeightedErrorSum(ActivationLayerBackwardCompute backwardCompute, float* valueBuffer, int outputIndex)
        {
            Contract.Requires(backwardCompute != null);

            // Compute weighted error sum:
            float sum = 0.0f;

            if (backwardCompute.OutputErrorBuffer.HasValue) // This is the output layer
            {
                // External errors are set:
                int errorValueIndex = backwardCompute.OutputErrorBuffer.Value.MinValue + outputIndex;
                sum = valueBuffer[errorValueIndex];
            }

            foreach (var lowerErrorAccessItem in backwardCompute.LowerErrorValueAccessItems)
            {
                for (int outputErrorIndex = 0; outputErrorIndex < lowerErrorAccessItem.ErrorSize; outputErrorIndex++)
                {
                    int outputErrorValueIndex = lowerErrorAccessItem.ErrorBufferBeginIndex + outputErrorIndex;

                    sum += valueBuffer[outputErrorValueIndex] * valueBuffer[lowerErrorAccessItem.WeightBufferBeginIndex + WeightAccessor.GetWeightValueIndex(outputIndex, outputErrorIndex, lowerErrorAccessItem.ErrorSize)];
                }
            }
            return sum;
        }

        private static unsafe void SetGradients(ActivationLayerBackwardCompute backwardCompute, float* valueBuffer, int outputIndex, float error)
        {
            // Bias:
            valueBuffer[backwardCompute.BiasGradientValueIndex.Value] = error;
            valueBuffer[backwardCompute.BiasGradientSumValueIndex.Value] += error;

            // Inputs:
            int outputSize = backwardCompute.ForwardCompute.OutputBuffer.Size;
            int layerIndex = 0;
            fixed (IntRange* gradientBuffers = backwardCompute.GradientBuffers)
            fixed (IntRange* gradientSumBuffers = backwardCompute.GradientSumBuffers)
            {
                var gradientBuff = gradientBuffers[layerIndex];
                var gradientSumBuff = gradientSumBuffers[layerIndex];

                foreach (var inputAccess in backwardCompute.ForwardCompute.InputValueAccessItems)
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
    }
}
