using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow
{
    internal class ManagedArray : IDeviceArray
    {
        internal ManagedArray(float[] array)
        {
            Debug.Assert(array != null && array.Length > 0);

            internalArray = array;
            Size = array.Length;
        }

        internal ManagedArray(int size)
        {
            Debug.Assert(size > 0);

            internalArray = new float[size];
            Size = size;
        }

        internal ManagedArray(ManagedDeviceArrayPool pool, int beginIndex, int size)
        {
            Debug.Assert(size > 0);
            Debug.Assert(beginIndex >= 0);
            Debug.Assert(pool != null);

            this.pool = pool;
            BeginIndex = beginIndex;
            Size = size;
        }

        ManagedDeviceArrayPool pool;

        float[] internalArray;

        internal float[] InternalArray
        {
            get { return internalArray ?? pool.InternalArray; }
        }

        internal int BeginIndex { get; private set; }

        public int Size { get; private set; }

        public virtual DeviceArrayType Type
        {
            get { return DeviceArrayType.DeviceArray; }
        }

        unsafe internal ManagedArrayPtr ToPtr(float* ptr)
        {
            return new ManagedArrayPtr(this, ptr, BeginIndex);
        }
    }
}
