using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;

namespace Neuroflow.Core.ComputationAPI
{
    public static class ValueSpace
    {
        #region Double

        unsafe public static double FastGet(this ValueSpace<double> valueSpace, int index)
        {
            return ((double*)valueSpace.Ptr)[index];
        }

        unsafe public static void FastSet(this ValueSpace<double> valueSpace, int index, double value)
        {
            ((double*)valueSpace.Ptr)[index] = value;
        }

        #endregion

        #region Bool

        unsafe public static bool FastGet(this ValueSpace<bool> valueSpace, int index)
        {
            return ((bool*)valueSpace.Ptr)[index];
        }

        unsafe public static void FastSet(this ValueSpace<bool> valueSpace, int index, bool value)
        {
            ((bool*)valueSpace.Ptr)[index] = value;
        }

        #endregion
    }

    public sealed class ValueSpace<T> : IDisposable where T : struct
    {
        public class ValueRef
        {
            public string this[int index]
            {
                get
                {
                    Contract.Requires(index >= 0);
                    return ComputationBlock.ValueMemberName + "[" + index + "]";
                }
            }
        }

        public ValueSpace(Guid structuralUID)
        {
            Ref = new ValueRef();
            SizeOf = Marshal.SizeOf(typeof(T));
            StructuralUID = structuralUID;
        }

        object sync = new object();

        bool disposed;

        public Guid StructuralUID { get; private set; }

        public ValueRef Ref { get; private set; }

        public int SizeOf { get; private set; }

        bool allocated;

        public IntPtr Ptr { get; private set; }

        public T this[int index]
        {
            get
            {
                if (!allocated)
                {
                    lock (sync)
                    {
                        if (disposed) throw GetDisposedEx();
                        if (!allocated) throw GetNotClosedEx();
                    }
                }

                return (T)Marshal.PtrToStructure(Ptr + (index * SizeOf), typeof(T));
            }
            set
            {
                if (!allocated)
                {
                    lock (sync)
                    {
                        if (disposed) throw GetDisposedEx();
                        if (!allocated) throw GetNotClosedEx();
                    }
                }

                Marshal.StructureToPtr(value, Ptr + (index * SizeOf), false);
            }
        }

        public bool IsClosed
        {
            get
            {
                lock (sync)
                {
                    if (disposed) throw GetDisposedEx(); 
                    return allocated;
                }
            }
        }

        int declIndex;

        public int Declare()
        {
            Contract.Ensures(Contract.Result<int>() >= 0);

            if (IsClosed) throw GetAlreadyClosedEx();
            
            return declIndex++;
        }
        
        public int[] Declare(int count)
        {
            Contract.Requires(count > 0 && count < 100000);
            Contract.Ensures(Contract.Result<int[]>() != null);
            Contract.Ensures(Contract.Result<int[]>().Length == count);
            Contract.Ensures(Contract.ForAll<int>(Contract.Result<int[]>(), (v) => v < declIndex));

            if (IsClosed) throw GetAlreadyClosedEx();

            var result = new int[count];
            for (int i = 0; i < count; i++, declIndex++)
            {
                result[i] = declIndex;
            }
            return result;
        }

        public void Close()
        {
            Contract.Ensures(allocated);

            CloseInternal();
        }

        internal IntPtr CloseInternal()
        {
            Contract.Ensures(allocated);

            lock (sync)
            {
                if (disposed) throw GetDisposedEx();
                if (!allocated)
                {
                    int size = declIndex * SizeOf;
                    Ptr = Marshal.AllocHGlobal(size);
                    Rtl.ZeroMemory(Ptr, size);
                    allocated = true;
                }
                return Ptr;
            }
        }

        private static Exception GetAlreadyClosedEx()
        {
            return new InvalidOperationException("Value Space already closed.");
        }

        private static Exception GetNotClosedEx()
        {
            return new InvalidOperationException("Value Space is not closed.");
        }

        private static Exception GetDisposedEx()
        {
            return new ObjectDisposedException("Value Space is disposed.");
        }

        public void Dispose()
        {
            Dispose(true);
        }

        void Dispose(bool isDisposing)
        {
            IntPtr? toFree = null;

            if (isDisposing)
            {
                if (Ptr != null)
                {
                    lock (sync)
                    {
                        if (allocated)
                        {
                            toFree = Ptr;
                            allocated = false;
                            disposed = true;
                        }
                    }
                }
            }
            else
            {
                toFree = Ptr;
            }

            if (toFree != null) Marshal.FreeHGlobal(toFree.Value);
        }

        ~ValueSpace()
        {
            Dispose(false);
        }
    }
}
