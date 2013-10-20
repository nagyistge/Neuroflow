using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Neuroflow.Data
{
    public delegate void DataArrayCompletedCallback(Exception ex);

    public abstract unsafe class DataArray : DisposableObject, IDeviceArray
    {
        public abstract bool IsConst { get; }

        public abstract int Size { get; }

        public DeviceArrayType Type
        {
            get { return DeviceArrayType.DataArray; }
        }

        public Task Read(float[] targetArray)
        {
            Args.Requires(() => targetArray, () => targetArray != null && targetArray.Length >= Size);

            return Read(0, targetArray.Length, targetArray, 0);
        }

        public Task Write(float[] sourceArray)
        {
            Args.Requires(() => sourceArray, () => sourceArray != null && sourceArray.Length >= Size);

            return Write(sourceArray, 0, sourceArray.Length, 0);
        }

        public Task Read(int sourceBeginIndex, int count, float[] targetArray, int targetBeginIndex)
        {
            Args.Requires(() => sourceBeginIndex, () => sourceBeginIndex >= 0 && sourceBeginIndex < Size);
            Args.Requires(() => count, () => count > 0 && count <= Size);
            Args.Requires(() => targetArray, () => targetArray != null && targetArray.Length <= Size);
            Args.Requires(() => targetBeginIndex, () => targetBeginIndex >= 0 && targetBeginIndex < targetArray.Length);

            var compl = new TaskCompletionSource<object>();
            var arrayH = new GCHandleRef(GCHandle.Alloc(targetArray, GCHandleType.Pinned));
            var doneH = new GCHandleRef();
            DataArrayCompletedCallback doneFunc = null;
            doneFunc = new DataArrayCompletedCallback(ex =>
            {
                try
                {
                    if (ex == null) compl.SetResult(null); else compl.SetException(ex);
                }
                finally
                {
                    arrayH.Handle.Free();
                    doneH.Handle.Free();
                }
            });
            doneH.Handle = GCHandle.Alloc(doneFunc);
            try
            {
                ReadAsync(sourceBeginIndex, count, (float*)arrayH.Handle.AddrOfPinnedObject(), targetBeginIndex, doneFunc);
                return compl.Task;
            }
            catch
            {
                arrayH.Handle.Free();
                doneH.Handle.Free();
                throw;
            }
        }

        protected abstract void ReadAsync(int sourceBeginIndex, int count, float* targetPtr, int targetBeginIndex, DataArrayCompletedCallback doneFunc);

        public Task Write(float[] sourceArray, int sourceBeginIndex, int count, int targetBeginIndex)
        {
            Args.Requires(() => sourceArray, () => sourceArray != null && sourceArray.Length <= Size);
            Args.Requires(() => sourceBeginIndex, () => sourceBeginIndex >= 0 && sourceBeginIndex < sourceArray.Length);
            Args.Requires(() => count, () => count > 0 && count <= Size);
            Args.Requires(() => targetBeginIndex, () => targetBeginIndex >= 0 && targetBeginIndex < Size);

            var compl = new TaskCompletionSource<object>();
            var arrayH = new GCHandleRef(GCHandle.Alloc(sourceArray, GCHandleType.Pinned));
            var doneH = new GCHandleRef();
            DataArrayCompletedCallback doneFunc = null;
            doneFunc = new DataArrayCompletedCallback(ex =>
            {
                try
                {
                    if (ex == null) compl.SetResult(null); else compl.SetException(ex);
                }
                finally
                {
                    arrayH.Handle.Free();
                    doneH.Handle.Free();
                }
            });
            doneH.Handle = GCHandle.Alloc(doneFunc);
            try
            {
                WriteAsync((float*)arrayH.Handle.AddrOfPinnedObject(), sourceBeginIndex, count, targetBeginIndex, doneFunc);
                return compl.Task;
            }
            catch
            {
                arrayH.Handle.Free();
                doneH.Handle.Free();
                throw;
            }
        }

        protected abstract void WriteAsync(float* sourcePtr, int sourceBeginIndex, int count, int targetBeginIndex, DataArrayCompletedCallback doneFunc);
    }
}
