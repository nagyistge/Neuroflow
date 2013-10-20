using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.NeuralNetworks
{
    internal sealed class DeviceArrayStack : IndexedResourceBag<IDeviceArray>
    {
        internal DeviceArrayStack(IDeviceArrayManagement daMan, int arraySize) :
            base(() => daMan.CreateArray(false, arraySize))
        {
            Debug.Assert(daMan != null);
            Debug.Assert(arraySize > 0);
            this.arraySize = arraySize;
            this.daMan = daMan;
        }

        IDeviceArrayManagement daMan;

        int arraySize;

        internal void Push(int index, IDeviceArray deviceArray)
        {
            Debug.Assert(deviceArray != null);
            Debug.Assert(deviceArray.Size == arraySize);
            Debug.Assert(index >= 0);

            var to = this[index];
            daMan.Copy(deviceArray, 0, to, 0, arraySize);
        }
    }
}
