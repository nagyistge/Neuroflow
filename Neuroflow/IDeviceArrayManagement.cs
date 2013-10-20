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

        unsafe void Copy(IDeviceArray from, int fromIndex, IDeviceArray to, int toIndex, int size);        
    }
}
