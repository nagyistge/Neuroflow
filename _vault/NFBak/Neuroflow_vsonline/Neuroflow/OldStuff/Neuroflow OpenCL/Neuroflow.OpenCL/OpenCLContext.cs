using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cloo;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace Neuroflow.OpenCL
{
    public class OpenCLContext : IDisposable
    {
        #region Static Wrappers

        public static ReadOnlyCollection<ComputePlatform> Platforms
        {
            get { return ComputePlatform.Platforms; }
        }

        public static ComputePlatform DefaultPlatform
        {
            get
            {
                if (Platforms.Count == 0) throw new OpenCLException("There are no platforms available.");
                return Platforms[0];
            }
        }

        #endregion

        #region Construct

        public OpenCLContext()
            : this(ComputeDeviceTypes.All, DefaultPlatform)
        {
        }

        public OpenCLContext(ComputeDeviceTypes deviceType, ComputePlatform platform)
        {
            Contract.Requires(platform != null);

            ComputeContext = new ComputeContext(deviceType, new ComputeContextPropertyList(platform), null, IntPtr.Zero);
        } 

        #endregion

        #region Fields and Props
        
        public ComputeContext ComputeContext { get; private set; }

        public ReadOnlyCollection<ComputeDevice> Devices
        {
            get { return ComputeContext.Devices; }
        }

        public ComputeDevice DefaultDevice
        {
            get
            {
                if (Devices.Count == 0) throw new OpenCLException("There are no devices available.");
                return Devices[0];
            }
        }

        #endregion

        #region Program

        public OpenCLProgram CompileProgram(params string[] source)
        {
            Contract.Requires(source.Length > 0);
            Contract.ForAll(source, (s) => !string.IsNullOrEmpty(s));

            return DoCompileProgram(new[] { DefaultDevice }, source);
        }

        public OpenCLProgram CompileProgram(IList<ComputeDevice> devices, params string[] source)
        {
            Contract.Requires(devices != null);
            Contract.Requires(devices.Count > 0);
            Contract.Requires(source.Length > 0);
            Contract.ForAll(source, (s) => !string.IsNullOrEmpty(s));

            return DoCompileProgram(new[] { DefaultDevice }, source);
        }

        private OpenCLProgram DoCompileProgram(IList<ComputeDevice> devices, string[] source)
        {
            Contract.Requires(devices != null);
            Contract.Requires(devices.Count > 0);
            Contract.Requires(source.Length > 0);
            Contract.ForAll(source, (s) => !string.IsNullOrEmpty(s));

            var prog = new ComputeProgram(ComputeContext, source);
            Exception inner = null;
            bool done = false;

            try
            {
                prog.Build(devices, "", null, IntPtr.Zero);
                done = true;
            }
            catch (Exception ex)
            {
                inner = ex;
            }

            if (!done)
            {
                var logs = new LinkedList<ProgramBuildLog>();
                foreach (var d in devices)
                {
                    var logStr = prog.GetBuildLog(d);
                    logs.AddLast(new ProgramBuildLog(d, logStr));
                }
                throw new OpenCLBuildException(logs.ToArray(), inner);
            }

            return new OpenCLProgram(this, prog);
        }

        #endregion

        #region Buffer

        public OpenCLBuffer<T> CreateBuffer<T>(long size, ComputeMemoryFlags flags) where T : struct
        {
            Contract.Requires(size > 0);

            var cb = new ComputeBuffer<T>(ComputeContext, flags, size);
            return new OpenCLBuffer<T>(this, cb);
        }

        public OpenCLBuffer<T> CreateBuffer<T>(T[] initData, ComputeMemoryFlags flags) where T : struct
        {
            Contract.Requires(initData != null);
            Contract.Requires(initData.Length > 0);

            var cb = new ComputeBuffer<T>(ComputeContext, flags, initData);
            return new OpenCLBuffer<T>(this, cb);
        }

        public OpenCLSubBuffer<T> CreateSubBuffer<T>(OpenCLBuffer<T> baseBuffer, ComputeMemoryFlags flags, long offset, long count) where T : struct
        {
            Contract.Requires(baseBuffer != null);
            Contract.Requires(offset >= 0);
            Contract.Requires(count > 0);

            var cb = new ComputeSubBuffer<T>(baseBuffer.ComputeBuffer, flags, offset, count);
            return new OpenCLSubBuffer<T>(this, baseBuffer, cb);
        }

        #endregion

        #region Queue

        public OpenCLQueue CreateQueue(ComputeCommandQueueFlags properties = ComputeCommandQueueFlags.None)
        {
            return CreateQueue(DefaultDevice, properties);
        }

        public OpenCLQueue CreateQueue(ComputeDevice device, ComputeCommandQueueFlags properties = ComputeCommandQueueFlags.None)
        {
            Contract.Requires(device != null);

            return new OpenCLQueue(this, new ComputeCommandQueue(ComputeContext, device, properties));
        }

        #endregion

        #region Dispose

        public void Dispose()
        {
            ComputeContext.Dispose();
        } 

        #endregion
    }
}
