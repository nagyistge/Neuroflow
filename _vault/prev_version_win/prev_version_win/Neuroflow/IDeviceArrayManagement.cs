using Neuroflow.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow
{
    public interface IDeviceArrayManagement
    {
        IDeviceArray CreateArray(bool copyOptimized, int size);

        IDeviceArray2 CreateArray2(bool copyOptimized, int rowSize, int colSize);

        void Copy(IDeviceArray from, int fromIndex, IDeviceArray to, int toIndex, int size);

        IDeviceArrayPool CreatePool();
    }
}
