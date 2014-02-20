using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core;
using System.Diagnostics.Contracts;

namespace Neuroflow.Networks.Neural.CPU
{
    internal static class ValueBuffer
    {
        unsafe internal static void Zero(float* valueBuffer, IntRange range)
        {
            Rtl.ZeroMemory((IntPtr)(valueBuffer + range.MinValue * sizeof(float)), new IntPtr(range.Size * sizeof(float)));
        }

        internal static void Zero(float[] valueBuffer, IntRange range)
        {
            Contract.Requires(valueBuffer != null);
            Contract.Requires(valueBuffer.Length >= range.MinValue);
            Contract.Requires(valueBuffer.Length >= range.MaxValue);

            Array.Clear(valueBuffer, range.MinValue, range.Size);
        }
    }
}
