using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow
{
    internal sealed class ManagedDeviceArrayPool : IDeviceArrayPool
    {
        int endIndex;

        float[] internalArray;

        public float[] InternalArray
        {
            get
            {
                Allocate();
                Debug.Assert(internalArray != null);
                Debug.Assert(IsAllocated);
                return internalArray;
            }
        }

        public int Size
        {
            get { return endIndex; }
        }

        public bool IsAllocated
        {
            get { return internalArray != null; }
        }

        public IDeviceArray CreateArray(bool copyOptimized, int size)
        {
            return new ManagedArray(this, Reserve(size), size);
        }

        public IDeviceArray2 CreateArray2(bool copyOptimized, int rowSize, int colSize)
        {
            return new ManagedArray2(this, Reserve(rowSize * colSize), rowSize, colSize);
        }

        public void Allocate()
        {
            if (endIndex == 0) throw new InvalidOperationException("There is no allocated memory in the pool.");
            if (!IsAllocated) internalArray = new float[Size];
        }

        public void Zero()
        {
            if (!IsAllocated) throw new InvalidOperationException("Cannot zero out an unallocated pool.");
            ManagedVectorUtils.ZeroMemory(internalArray);
        }

        private int Reserve(int size)
        {
            if (IsAllocated) throw new InvalidOperationException("Cannot reserve memory in an already allocated pool.");
            int beginIndex = endIndex;
            endIndex += size;
            return beginIndex;
        }
    }
}
