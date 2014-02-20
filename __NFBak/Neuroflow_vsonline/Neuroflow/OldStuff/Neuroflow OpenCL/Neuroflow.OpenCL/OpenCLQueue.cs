using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cloo;
using System.Diagnostics.Contracts;

namespace Neuroflow.OpenCL
{
    public class OpenCLQueue : IDisposable
    {
        #region Construct

        internal OpenCLQueue(OpenCLContext context, ComputeCommandQueue computeCommandQueue)
        {
            Contract.Requires(context != null);
            Contract.Requires(computeCommandQueue != null);

            Context = context;
            ComputeCommandQueue = computeCommandQueue;
        } 

        #endregion

        #region Fields and Props

        public OpenCLContext Context { get; private set; }

        public ComputeCommandQueue ComputeCommandQueue { get; private set; } 

        #endregion

        #region Execute

        public void ExecuteTask(OpenCLKernel kernel, ICollection<ComputeEventBase> events = null)
        {
            ComputeCommandQueue.ExecuteTask(kernel.ComputeKernel, events);
        }

        public void Execute(OpenCLKernel kernel, long[] globalWorkSize, long[] localWorkSize = null, ICollection<ComputeEventBase> events = null)
        {
            ExecuteWithOffset(kernel, null, globalWorkSize, localWorkSize, events);
        }

        public void ExecuteWithOffset(OpenCLKernel kernel, long[] globalWorkOffset, long[] globalWorkSize, long[] localWorkSize = null, ICollection<ComputeEventBase> events = null)
        {
            Contract.Requires(kernel != null);

            ComputeCommandQueue.Execute(kernel.ComputeKernel, globalWorkOffset, globalWorkSize, localWorkSize, events);
        } 

        #endregion

        #region Read

        public void ReadAll(OpenCLBufferBase<float> buffer, float[] targetArray, bool blocking = true, IList<ComputeEventBase> events = null)
        {
            Contract.Requires(targetArray != null);
            Contract.Requires(targetArray.Length > 0);

            ComputeCommandQueue.ReadFromBuffer(buffer.ComputeBufferBase, ref targetArray, blocking, events);
        }

        public void Read(OpenCLBufferBase<float> buffer, float[] targetArray, long offset, long count, bool blocking = true, IList<ComputeEventBase> events = null)
        {
            Contract.Requires(targetArray != null);
            Contract.Requires(targetArray.Length > 0);
            Contract.Requires(offset >= 0);
            Contract.Requires(count > 0);

            ComputeCommandQueue.ReadFromBuffer(buffer.ComputeBufferBase, ref targetArray, blocking, offset, 0, count, events);
        }

        #endregion

        #region Write

        public void WriteAll(OpenCLBufferBase<float> buffer, float[] targetArray, bool blocking = true, IList<ComputeEventBase> events = null)
        {
            Contract.Requires(targetArray != null);
            Contract.Requires(targetArray.Length > 0);

            ComputeCommandQueue.WriteToBuffer(targetArray, buffer.ComputeBufferBase, blocking, events);
        }

        public void Write(OpenCLBufferBase<float> buffer, float[] targetArray, long offset, long count, bool blocking = true, IList<ComputeEventBase> events = null)
        {
            Contract.Requires(targetArray != null);
            Contract.Requires(targetArray.Length > 0);
            Contract.Requires(offset >= 0);
            Contract.Requires(count > 0);

            ComputeCommandQueue.WriteToBuffer(targetArray, buffer.ComputeBufferBase, blocking, 0, offset, count, events);
        }

        #endregion

        #region Dispose

        public void Dispose()
        {
            ComputeCommandQueue.Dispose();
        } 

        #endregion
    }
}
