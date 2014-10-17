using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Cloo;

namespace Neuroflow.OpenCL
{
    public abstract class OpenCLBufferBase<T> : OpenCLMemory
        where T : struct
    {
        protected OpenCLBufferBase(OpenCLContext context, ComputeBufferBase<T> bufferBase)
            : base(context, bufferBase)
        {
            Contract.Requires(context != null);
            Contract.Requires(bufferBase != null);
        }

        public ComputeBufferBase<T> ComputeBufferBase
        {
            get { return (ComputeBufferBase<T>)base.ComputeMemory; }
        }
    }
}
