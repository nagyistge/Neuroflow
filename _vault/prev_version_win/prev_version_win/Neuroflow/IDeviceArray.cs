using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow
{
    public interface IDeviceArray
    {
        DeviceArrayType Type { get; }

        int Size { get; }
    }
}
