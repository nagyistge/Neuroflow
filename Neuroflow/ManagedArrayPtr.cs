using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow
{
    unsafe internal struct ManagedArrayPtr
    {
        internal ManagedArrayPtr(ManagedArray array, float* ptr, int beginIndex)
        {
            Debug.Assert(array != null && ptr != null);

            this.array = array;
            this.ptr = ptr;
            this.beginIndex = beginIndex;
        }

        public static ManagedArrayPtr Null = new ManagedArrayPtr();

        ManagedArray array;
        
        float* ptr;

        int beginIndex;

        internal int Size
        {
            get { return array.Size; }
        }

#if DEBUG
        internal float this[int i]
        {
            get
            {
                return array.InternalArray[i + beginIndex];
            }
            set
            {
                array.InternalArray[i + beginIndex] = value;
            }
        }
#else
        internal float this[int i]
        {
            get
            {
                return (ptr + beginIndex)[i];
            }
            set
            {
                (ptr + beginIndex)[i] = value;
            }
        }
#endif
    }
}
