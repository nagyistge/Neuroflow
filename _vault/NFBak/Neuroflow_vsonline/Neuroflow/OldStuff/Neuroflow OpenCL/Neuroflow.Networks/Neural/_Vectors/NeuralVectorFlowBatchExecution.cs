using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core.Vectors;
using System.Diagnostics.Contracts;
using System.Diagnostics;
using Neuroflow.Networks.Neural.Learning;

namespace Neuroflow.Networks.Neural
{
    [Serializable]
    public abstract class NeuralVectorFlowBatchExecution : VectorFlowBatchExecution<float>
    {
        const double ErrorScale = 0.5;

        protected NeuralVectorFlowBatchExecution(NeuralNetwork network, IterationRepeatPars repeatPars)
            : base(network, repeatPars)
        {
            Contract.Requires(network != null);

            Network = network;
        }

        public NeuralNetwork Network { get; private set; }

        [NonSerialized]
        float[] outputBuffer, errorBuffer;

        private float[] OutputBuffer
        {
            get { return outputBuffer ?? (outputBuffer = new float[Network.OutputInterfaceLength]); }
        }

        internal float[] ErrorBuffer
        {
            get { return errorBuffer ?? (errorBuffer = new float[Network.OutputInterfaceLength]); }
        }

        protected unsafe override double? ComputeError(float?[] desiredOutputVector, int entryIndex, int entryCount)
        {
            Debug.Assert(ErrorBuffer.Length == desiredOutputVector.Length);
            Debug.Assert(Network.OutputInterfaceLength == desiredOutputVector.Length);

            // Read output:
            Network.ReadOutput(OutputBuffer);

            double count = 0.0;
            double sum = 0.0;
            fixed (float* pOutputBuffer = OutputBuffer, pErrorBuffer = ErrorBuffer)
            {
                for (int idx = 0; idx < desiredOutputVector.Length; idx++)
                {
                    float? desiredValue = desiredOutputVector[idx];
                    if (desiredValue.HasValue)
                    {
                        float currentOutputValue = pOutputBuffer[idx];
                        float error = desiredValue.Value - currentOutputValue;
                        pErrorBuffer[idx] = error;
                        sum += Math.Pow(error * ErrorScale, 2.0);
                        count++;
                    }
                    else
                    {
                        pErrorBuffer[idx] = 0;
                    }
                }
            }

            bool anyError = count != 0.0;

            return anyError ? ((sum / count) / 2.0) : (double?)null;
        }
    }
}
