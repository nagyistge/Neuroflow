using Neuroflow.Core;
using Neuroflow.Core.Vectors;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.Networks.Neural
{
    public sealed class ManagedBufferOps : IBufferOps
    {
        unsafe public void Zero(NeuralComputationContext context, IntRange range)
        {
            var valueBuffer = GetValueBuff(context);

            ValueBuffer.Zero(valueBuffer, range);
        }

        unsafe public void ZeroAll(NeuralComputationContext context)
        {
            var valueBuffer = GetValueBuff(context);
            fixed (float* pValueBuffer = valueBuffer)
            {
                Rtl.ZeroMemory((IntPtr)pValueBuffer, (IntPtr)(sizeof(float) * valueBuffer.Length));
            }
        }

        unsafe public void CopyBuffer(NeuralComputationContext context, IntRange source, IntRange target)
        {
            var valueBuffer = GetValueBuff(context);

            fixed (float* pValueBuffer = valueBuffer)
            {
                int size = source.Size;
                for (int i = 0; i < size; i++)
                {
                    pValueBuffer[target.MinValue + i] = pValueBuffer[source.MinValue + i];
                }
            }
        }
        
        unsafe public void WriteVector(NeuralComputationContext context, IntRange range, BufferedVector<float> values)
        {
            int len = values.Length;
            int begin = range.MinValue;
            var valueBuffer = GetValueBuff(context);
            fixed (float* pValues = GetValueArray(values), pValueBuffer = valueBuffer)
            {
                for (int i = 0; i < len; i++)
                {
                    pValueBuffer[begin + i] = pValues[i];
                }
            }
        }

        unsafe public void ReadArray(NeuralComputationContext context, IntRange range, float[] values)
        {
            int len = values.Length;
            int begin = range.MinValue;
            var valueBuffer = GetValueBuff(context);
            fixed (float* pValues = values, pValueBuffer = valueBuffer)
            {
                for (int i = 0; i < len; i++)
                {
                    pValues[i] = pValueBuffer[begin + i];
                }
            }
        }

        unsafe public void ComputeError(NeuralComputationContext context, IntRange outputBuffer, BufferedVector<float> desiredOutputVector, IntRange errorBuffer, IntRange accumulationBuffer)
        {
            var valueBuffer = GetValueBuff(context);
            var mvb = desiredOutputVector.Owner as ManagedVectorBuffer<float>;
            var desiredOutputValues = mvb.GetArray(desiredOutputVector);

            fixed (float* pValueBuffer = valueBuffer, pDesiredOutputValues = desiredOutputValues)
            {
                int outputBegin = outputBuffer.MinValue;
                int errorBegin = errorBuffer.MinValue;
                int accBegin = accumulationBuffer.MinValue;
                int accCount = accumulationBuffer.MaxValue;
                for (int idx = 0; idx < desiredOutputVector.Length; idx++)
                {
                    float desiredValue = pDesiredOutputValues[idx];
                    float currentOutputValue = pValueBuffer[outputBegin + idx];
                    float error = desiredValue - currentOutputValue;
                    pValueBuffer[errorBegin + idx] = error;
                    pValueBuffer[accBegin + idx] += (float)Math.Pow(error * 0.5, 2.0);
                }
                pValueBuffer[accCount]++;
            }
        }

        unsafe public void CalculateAverageError(NeuralComputationContext context, IntRange accumulationBuffer)
        {
            var valueBuffer = GetValueBuff(context);

            fixed (float* pValueBuffer = valueBuffer)
            {
                int accBegin = accumulationBuffer.MinValue;
                float accCount = pValueBuffer[accumulationBuffer.MaxValue];
                if (accCount != 0.0f)
                {
                    int size = accumulationBuffer.Size - 1;
                    for (int idx = 0; idx < size; idx++)
                    {
                        pValueBuffer[accBegin + idx] = pValueBuffer[accBegin + idx] / accCount;
                    }
                }
            }
        }

        unsafe public void Average(NeuralComputationContext context, int outputIndex, IntRange range)
        {
            var valueBuffer = GetValueBuff(context);
            fixed (float* pValueBuffer = valueBuffer)
            {
                double averageError = ValueBuffer.Average(pValueBuffer, range);
                pValueBuffer[outputIndex] = (float)averageError;
            }
        }

        unsafe public void AverageDist(NeuralComputationContext context, int outputIndex, IntRange range)
        {
            var valueBuffer = GetValueBuff(context);
            fixed (float* pValueBuffer = valueBuffer)
            {
                double averageError = ValueBuffer.AverageDist(pValueBuffer, range);
                pValueBuffer[outputIndex] = (float)averageError;
            }
        }

        internal static float[] GetValueArray(BufferedVector<float> values)
        {
            Contract.Requires(values != null);

            var mvb = values.Owner as ManagedVectorBuffer<float>;
            if (mvb == null) throw new InvalidOperationException("Vector source is unknown.");
            return mvb.GetArray(values);
        }

        internal static float[] GetValueBuff(NeuralComputationContext context)
        {
            Contract.Requires(context != null);

            try
            {
                return ((ManagedNeuralComputationContext)context).Buff;
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Invalid context.", ex);
            }
        }
    }
}
