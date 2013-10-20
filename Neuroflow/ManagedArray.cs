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

            InternalArray = array;
        }

        internal ManagedArray(int size)
        {
            Debug.Assert(size > 0);

            InternalArray = new float[size];
        }

        internal float[] InternalArray { get; private set; }

        public int Size
        {
            get { return InternalArray.Length; }
        }

        public virtual DeviceArrayType Type
        {
            get { return DeviceArrayType.DeviceArray; }
        }

        unsafe internal ManagedArrayPtr ToPtr(float* ptr)
        {
            return new ManagedArrayPtr(this, ptr);
        }

        [Conditional("Debug")]
        internal void Dump()
        {
            Debug.WriteLine(string.Format("{0}: {1}", Size, string.Join(" ", InternalArray)));
        }
    }
}
