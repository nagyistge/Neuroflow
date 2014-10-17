using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow
{
    internal sealed class ManagedArray2 : ManagedArray, IDeviceArray2
    {
        internal ManagedArray2(int size1, int size2) :
            base(size1 * size2)
        {
            Debug.Assert(size1 > 0);
            Debug.Assert(size2 > 0);

            Size1 = size1;
            Size2 = size2;
        }

        internal ManagedArray2(ManagedDeviceArrayPool pool, int beginIndex, int size1, int size2) :
            base(pool, beginIndex, size1 * size2)
        {
            Debug.Assert(size1 > 0);
            Debug.Assert(size2 > 0);

            Size1 = size1;
            Size2 = size2;
        }

        public int Size1 { get; private set; }

        public int Size2 { get; private set; }

        public override DeviceArrayType Type
        {
            get
            {
                return DeviceArrayType.DeviceArray2;
            }
        }

        unsafe internal ManagedArray2Ptr ToPtr2(float* ptr)
        {
            return new ManagedArray2Ptr(this, ptr, BeginIndex);
        }
    }
}
