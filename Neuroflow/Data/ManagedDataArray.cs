using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.Data
{
    internal sealed class ManagedDataArray : DataArray
    {
        internal ManagedDataArray(float[] array, bool isConst)
        {
            Debug.Assert(array != null && array.Length > 0);

            InternalManagedArray = new ManagedArray(array);
            this.isConst = isConst;
        }

        internal ManagedArray InternalManagedArray { get; private set; }

        bool isConst;

        public override bool IsConst
        {
            get { return isConst; }
        }

        public override int Size
        {
            get { return InternalManagedArray.Size; }
        }

        protected override unsafe void ReadAsync(int sourceBeginIndex, int count, float* targetPtr, int targetBeginIndex, DataArrayCompletedCallback doneFunc)
        {
            VerifyIsNotConst();

            try
            {
                Marshal.Copy(InternalManagedArray.InternalArray, sourceBeginIndex, new IntPtr(targetPtr + targetBeginIndex), count);
                doneFunc(null);
            }
            catch (Exception ex)
            {
                doneFunc(ex);
            }
        }

        protected override unsafe void WriteAsync(float* sourcePtr, int sourceBeginIndex, int count, int targetBeginIndex, DataArrayCompletedCallback doneFunc)
        {
            VerifyIsNotConst();

            try
            {
                Marshal.Copy(new IntPtr(sourcePtr + sourceBeginIndex), InternalManagedArray.InternalArray, targetBeginIndex, count);
                doneFunc(null);
            }
            catch (Exception ex)
            {
                doneFunc(ex);
            }
        }

        private void VerifyIsNotConst()
        {
            if (IsConst) throw new NotSupportedException("Accessing const data array is not supported.");
        }
    }
}
