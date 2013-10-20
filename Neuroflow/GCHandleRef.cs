using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow
{
    internal sealed class GCHandleRef
    {
        internal GCHandleRef()
        {
        }

        internal GCHandleRef(GCHandle handle)
        {
            Handle = handle;
        }

        internal GCHandle Handle { get; set; }
    }
}
