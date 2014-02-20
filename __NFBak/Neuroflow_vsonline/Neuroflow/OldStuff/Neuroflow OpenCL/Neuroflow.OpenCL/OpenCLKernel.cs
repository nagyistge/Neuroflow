using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cloo;
using System.Diagnostics.Contracts;

namespace Neuroflow.OpenCL
{
    public class OpenCLKernel : IDisposable
    {
        internal OpenCLKernel(OpenCLProgram program, ComputeKernel kernel)
        {
            Contract.Requires(program != null);
            Contract.Requires(kernel != null);

            Program = program;
            ComputeKernel = kernel;
        }

        public OpenCLProgram Program { get; private set; }
        
        public ComputeKernel ComputeKernel { get; private set; }

        public void SetArguments(params OpenCLMemory[] buffers)
        {
            Contract.Requires(buffers != null);

            for (int idx = 0; idx < buffers.Length; idx++)
            {
                ComputeKernel.SetMemoryArgument(idx, buffers[idx].ComputeMemory);
            }
        }

        public void Dispose()
        {
            ComputeKernel.Dispose();
        }
    }
}
