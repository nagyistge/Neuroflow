using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cloo;
using System.Diagnostics.Contracts;

namespace Neuroflow.OpenCL
{
    public abstract class OpenCLMemory : IDisposable
    {
        protected OpenCLMemory(OpenCLContext context, ComputeMemory computeMemory)
        {
            Contract.Requires(context != null);
            Contract.Requires(computeMemory != null);

            Context = context;
            ComputeMemory = computeMemory;
        }

        #region Fields and Props

        public OpenCLContext Context { get; private set; }

        public ComputeMemory ComputeMemory { get; private set; }

        #endregion

        #region Dispose

        public void Dispose()
        {
            ComputeMemory.Dispose();
        }

        #endregion
    }
}
