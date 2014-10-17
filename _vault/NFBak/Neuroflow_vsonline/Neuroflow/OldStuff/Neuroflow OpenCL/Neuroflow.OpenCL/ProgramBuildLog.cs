using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cloo;
using System.Diagnostics.Contracts;

namespace Neuroflow.OpenCL
{
    public sealed class ProgramBuildLog
    {
        internal ProgramBuildLog(ComputeDevice computeDevice, string log)
        {
            Contract.Requires(computeDevice != null);

            ComputeDevice = computeDevice;
            Log = log;
        }
        
        public ComputeDevice ComputeDevice { get; private set; }

        public string Log { get; private set; }

        public override string ToString()
        {
            return string.Format("{0} - {1}", ComputeDevice.Name, Log);
        }
    }
}
