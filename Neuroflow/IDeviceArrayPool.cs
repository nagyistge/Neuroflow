using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow
{
    public interface IDeviceArrayPool
    {
        bool IsAllocated { get; }

        IDeviceArray CreateArray(bool copyOptimized, int size);

        IDeviceArray2 CreateArray2(bool copyOptimized, int rowSize, int colSize);

        void Allocate();

        void Zero();
    }
}
