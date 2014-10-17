using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Neuroflow.ComputationAPI
{
    public static class Rtl
    {
        [DllImport("kernel32.dll")]
        public static extern void ZeroMemory(IntPtr dst, int length);
    }
}
