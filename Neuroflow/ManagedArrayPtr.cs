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
        internal ManagedArrayPtr(ManagedArray array, float* ptr)
        {
            Debug.Assert(array != null && ptr != null);

            this.array = array;
            this.ptr = ptr;
        }

        public static ManagedArrayPtr Null = new ManagedArrayPtr();

        ManagedArray array;
        
        float* ptr;

        internal int Size
        {
            get { return array.Size; }
        }

#if DEBUG
        internal float this[int i]
        {
            get
            {
                return array.InternalArray[i];
            }
            set
            {
                array.InternalArray[i] = value;
            }
        }
#else
        internal float this[int i]
        {
            get
            {
                return ptr[i];
            }
            set
            {
                ptr[i] = value;
            }
        }
#endif
    }
}
