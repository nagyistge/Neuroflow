using Neuroflow.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.Networks.Neural
{
    public sealed class ManagedBPTTOps : IBPTTOps
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

        unsafe public void ComputeForward(NeuralComputationContext context, int innerIterationIndex)
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

                            // With tracking, put inputs:
                            Debug.Assert(accessItem.InnerItarationInputValueStack != null);
                            Debug.Assert(accessItem.InnerItarationInputValueStack.Length > inputIndex);

                            var range = accessItem.InnerItarationInputValueStack[inputIndex];
                            pValueBuffer[range.MinValue + innerIterationIndex] = pValueBuffer[inputValueIndex];
                        }
                    }

                    // Set output derivate
                    // With tracking
                    Debug.Assert(forwardCompute.InnerItarationOutputValueStack != null);
                    Debug.Assert(forwardCompute.InnerItarationOutputValueStack.Length > outputIndex);

                    var stackBuffer = forwardCompute.InnerItarationOutputValueStack[outputIndex];

                    Debug.Assert(stackBuffer.Size > innerIterationIndex);

                    pValueBuffer[stackBuffer.MinValue + innerIterationIndex] = forwardCompute.Function.CalculateDerivate(pValueBuffer[forwardCompute.OutputBuffer.MinValue + outputIndex] = forwardCompute.Function.Calculate(sum));
                }
            }
        }

        unsafe public void ComputeBackwardInternalStep(NeuralComputationContext context, int innerIterationIndex)
        {
            var valueBuffer = ManagedBufferOps.GetValueBuff(context);
            fixed (float* pValueBuffer = valueBuffer)
            {
                for (int outputIndex = 0; outputIndex < backwardCompute.ForwardCompute.OutputBuffer.Size; outputIndex++)
                {
                    SetGradients_Int(backwardCompute, pValueBuffer, outputIndex, ComputeError(backwardCompute, pValueBuffer, outputIndex, innerIterationIndex), innerIterationIndex);
                }
            }
        }

        unsafe public void ComputeBackwardLastStep(NeuralComputationContext context, int innerIterationIndex)
        {
            var valueBuffer = ManagedBufferOps.GetValueBuff(context);
            fixed (float* pValueBuffer = valueBuffer)
            {
                for (int outputIndex = 0; outputIndex < backwardCompute.ForwardCompute.OutputBuffer.Size; outputIndex++)
                {
                    SetGradients_Last(backwardCompute, pValueBuffer, outputIndex, ComputeError(backwardCompute, pValueBuffer, outputIndex, innerIterationIndex), innerIterationIndex);
                }
            }
        }

        private static unsafe float ComputeError(ActivationLayerBackwardCompute backwardCompute, float* valueBuffer, int outputIndex, int innerIterationIndex)
        {
            float sum = ManagedFeedForwardOps.ComputeWeightedErrorSum(backwardCompute, valueBuffer, outputIndex);

            // Compute error:
            var innerItarationOutputValueStack = backwardCompute.ForwardCompute.InnerItarationOutputValueStack;

            // Recurrent, derivate is in the stack
            Debug.Assert(innerItarationOutputValueStack != null);
            Debug.Assert(innerItarationOutputValueStack.Length > outputIndex);

            var derivBuffer = innerItarationOutputValueStack[outputIndex];

            Debug.Assert(derivBuffer.Size > innerIterationIndex);

            return valueBuffer[backwardCompute.ErrorBuffer.Value.MinValue + outputIndex] =
                (sum * valueBuffer[derivBuffer.MinValue + innerIterationIndex]);
        }

        private static unsafe void SetGradients_Int(ActivationLayerBackwardCompute backwardCompute, float* valueBuffer, int outputIndex, float error, int innerItarationIndex)
        {
            // Bias:
            valueBuffer[backwardCompute.BiasGradientValueIndex.Value] += error;

            // Inputs:
            int outputSize = backwardCompute.ForwardCompute.OutputBuffer.Size;
            int layerIndex = 0;
            fixed (IntRange* gradientBuffers = backwardCompute.GradientBuffers)
            {
                var gradientBuff = gradientBuffers[layerIndex];

                foreach (var inputAccess in backwardCompute.ForwardCompute.InputValueAccessItems)
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

        private static unsafe void SetGradients_Last(ActivationLayerBackwardCompute backwardCompute, float* valueBuffer, int outputIndex, float error, int innerItarationIndex)
        {
            // Bias:
            valueBuffer[backwardCompute.BiasGradientValueIndex.Value] += error;
            valueBuffer[backwardCompute.BiasGradientSumValueIndex.Value] += valueBuffer[backwardCompute.BiasGradientValueIndex.Value];

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
    }
}
