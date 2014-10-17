using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cloo;
using System.Diagnostics.Contracts;

namespace Neuroflow.OpenCL
{
    public class OpenCLBuffer<T> : OpenCLBufferBase<T>
        where T : struct
    {
        #region Construct

        internal OpenCLBuffer(OpenCLContext context, ComputeBuffer<T> computeBuffer)
            : base(context, computeBuffer)
        {
            Contract.Requires(context != null);
            Contract.Requires(computeBuffer != null);
        } 

        #endregion

        #region Fields and Props

        public ComputeBuffer<T> ComputeBuffer
        {
            get { return (ComputeBuffer<T>)ComputeMemory; }
        }

        #endregion
    }
}
