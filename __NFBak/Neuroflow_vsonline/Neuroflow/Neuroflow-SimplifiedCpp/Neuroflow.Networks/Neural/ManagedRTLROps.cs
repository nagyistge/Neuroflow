using Neuroflow.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.Networks.Neural
{
    public sealed class ManagedRTLROps : IRTLROps
    {
        ActivationLayerForwardCompute forwardCompute;

        public void Initialize(ActivationLayerForwardCompute forwardCompute)
        {
            this.forwardCompute = forwardCompute;
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

                    // Store Net Value:
                    pValueBuffer[forwardCompute.NetDerivBuffer.Value.MinValue + outputIndex] = forwardCompute.Function.CalculateDerivate(sum);

                    // Set output derivate
                    pValueBuffer[forwardCompute.OutputBuffer.MinValue + outputIndex] = forwardCompute.Function.Calculate(sum);
                }
            }
        }
    }
}
