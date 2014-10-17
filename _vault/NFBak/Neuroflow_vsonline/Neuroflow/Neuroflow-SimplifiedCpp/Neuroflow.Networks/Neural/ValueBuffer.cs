using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core;
using System.Diagnostics.Contracts;

namespace Neuroflow.Networks.Neural
{
    internal static class ValueBuffer
    {
        internal static void Zero(float[] valueBuffer, IntRange range)
        {
            Contract.Requires(valueBuffer != null);
            Contract.Requires(valueBuffer.Length >= range.MinValue);
            Contract.Requires(valueBuffer.Length >= range.MaxValue);

            Array.Clear(valueBuffer, range.MinValue, range.Size);
        }

        internal static unsafe double Average(float* valueBuffer, IntRange errorBuffer)
        {
            double s = errorBuffer.Size;
            if (s == 0.0) return 0.0;
            double v = 0.0f;
            for (int i = errorBuffer.MinValue; i <= errorBuffer.MaxValue; i++) v += valueBuffer[i];
            return v / s;
        }

        internal static unsafe double AverageAccumulationBuffer(float* valueBuffer, IntRange accumulationBuffer)
        {
            double s = accumulationBuffer.Size;
            if (s == 0.0 || s == 1.0) return 0.0;
            double v = 0.0f;
            double c = valueBuffer[accumulationBuffer.MaxValue];
            if (c == 0.0f)
            {
                return v;
            }
            else
            {
                for (int i = accumulationBuffer.MinValue; i < accumulationBuffer.MaxValue; i++) v += valueBuffer[i] / c;
            }
            return v / (s - 1.0);
        }

        internal static unsafe double AverageDist(float* valueBuffer, IntRange errorBuffer)
        {
            double s = errorBuffer.Size;
            if (s == 0.0) return 0.0;
            double v = 0.0f;
            for (int i = errorBuffer.MinValue; i <= errorBuffer.MaxValue; i++) v += (float)Math.Pow(valueBuffer[i] * 0.5, 2.0);
            return v / s;
        }
    }
}
