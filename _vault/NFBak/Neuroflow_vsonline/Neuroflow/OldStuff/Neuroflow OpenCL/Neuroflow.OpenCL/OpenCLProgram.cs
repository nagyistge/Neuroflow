using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cloo;
using System.Diagnostics.Contracts;
using System.Threading;

namespace Neuroflow.OpenCL
{
    public sealed class OpenCLProgram : IDisposable
    {
        #region Construct

        internal OpenCLProgram(OpenCLContext context, ComputeProgram program)
        {
            Contract.Requires(context != null);
            Contract.Requires(program != null);

            Context = context;
            ComputeProgram = program;
        } 

        #endregion

        #region Fields and Props

        public OpenCLContext Context { get; private set; }

        public ComputeProgram ComputeProgram { get; private set; }

        #endregion

        #region Kernels

        public IEnumerable<OpenCLKernel> CreateAllKernels()
        {
            foreach (var ck in ComputeProgram.CreateAllKernels())
            {
                yield return new OpenCLKernel(this, ck);
            }
        }

        public OpenCLKernel CreateKernel(string functionName)
        {
            Contract.Requires(!String.IsNullOrEmpty(functionName));

            try
            {
                return new OpenCLKernel(this, ComputeProgram.CreateKernel(functionName));
            }
            catch (ComputeException ex)
            {
                throw new OpenCLException(string.Format("Kernel funtion '{0}' not found.", functionName), ex);
            }
        }

        #endregion

        #region Dispose

        public void Dispose()
        {
            ComputeProgram.Dispose();
        } 

        #endregion
    }
}
