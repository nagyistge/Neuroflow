using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cloo;
using System.Diagnostics.Contracts;

namespace Neuroflow.OpenCL
{
    public class OpenCLSubBuffer<T> : OpenCLBufferBase<T>
        where T : struct
    {
        #region Construct

        internal OpenCLSubBuffer(OpenCLContext context, OpenCLBuffer<T> baseBuffer, ComputeSubBuffer<T> computeSubBuffer)
            : base(context, computeSubBuffer)
        {
            Contract.Requires(context != null);
            Contract.Requires(computeSubBuffer != null);
            Contract.Requires(baseBuffer != null);

            BaseBuffer = baseBuffer;
        }

        #endregion

        #region Fields and Props

        public OpenCLBuffer<T> BaseBuffer { get; private set; }

        public ComputeSubBuffer<T> ComputeSubBuffer
        {
            get { return (ComputeSubBuffer<T>)ComputeMemory; }
        }

        #endregion
    }
}
