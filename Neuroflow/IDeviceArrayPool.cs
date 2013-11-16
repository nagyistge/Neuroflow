using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow
{
    public interface IDeviceArrayPool
    {
        int Size { get; }

        bool IsAllocated { get; }

        IDeviceArray CreateArray(int size);

        IDeviceArray2 CreateArray2(int rowSize, int colSize);

        void Allocate();

        void Zero();
    }
}
