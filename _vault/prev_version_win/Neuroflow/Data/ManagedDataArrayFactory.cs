using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.Data
{
    internal sealed class ManagedDataArrayFactory : DataArrayFactory
    {
        protected override DataArray DoCreate(int size, float fill)
        {
            var a = new float[size];
            if (fill != 0.0f) for (int i = 0; i < a.Length; i++) a[i] = fill;
            return new ManagedDataArray(a, false);
        }

        protected override DataArray DoCreate(float[] array, int beginPos, int length)
        {
            var a = new float[length];
            Array.Copy(array, beginPos, a, 0, length);
            return new ManagedDataArray(a, false);
        }

        protected override DataArray DoCreateConst(float[] array, int beginPos, int length)
        {
            var a = new float[length];
            Array.Copy(array, beginPos, a, 0, length);
            return new ManagedDataArray(a, true);
        }
    }
}
