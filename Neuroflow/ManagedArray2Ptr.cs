using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow
{
    unsafe internal struct ManagedArray2Ptr
    {
        internal ManagedArray2Ptr(ManagedArray2 array, float* ptr)
        {
            Debug.Assert(array != null && ptr != null);

            this.array = array;
            this.ptr = ptr;
        }

        ManagedArray2 array;

        float* ptr;

        internal int Size
        {
            get { return array.Size; }
        }

        internal int Size1
        {
            get { return array.Size1; }
        }

        internal int Size2
        {
            get { return array.Size2; }
        }

#if DEBUG
        internal float this[int upperIndex, int lowerIndex]
        {
            get
            {
                return array.InternalArray[lowerIndex * Size1 + upperIndex];
            }
            set
            {
                array.InternalArray[lowerIndex * Size1 + upperIndex] = value;
            }
        }
#else
        internal float this[int upperIndex, int lowerIndex]
        {
            get
            {
                return ptr[lowerIndex * Size1 + upperIndex];
            }
            set
            {
                ptr[lowerIndex * Size1 + upperIndex] = value;
            }
        }
#endif
    }
}
