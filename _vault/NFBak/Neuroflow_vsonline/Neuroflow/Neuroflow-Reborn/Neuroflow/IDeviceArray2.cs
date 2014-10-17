using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow
{
    public interface IDeviceArray2 : IDeviceArray
    {
        int Size1 { get; }

        int Size2 { get; }
    }
}
