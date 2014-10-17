using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.Core.Vectors
{
    public abstract class VectorBuffer<T> : SynchronizedObject, IDisposable
        where T : struct
    {
        #region Props and Fields

        bool disposed;

        #endregion

        #region Impl

        public BufferedVector<T> GetOrCreate(int rowIndex, int colIndex, Func<T[]> valuesFactory)
        {
            Contract.Requires(rowIndex >= 0);
            Contract.Requires(colIndex >= 0);
            Contract.Requires(valuesFactory != null);

            //lock (SyncRoot)
            {
                return DoGetOrCreate(rowIndex, colIndex, () =>
                {
                    var values = valuesFactory();
                    if (values.Length == 0) throw new InvalidOperationException("Vector values is empty.");
                    return values;
                });
            }
        }

        protected abstract BufferedVector<T> DoGetOrCreate(int rowIndex, int colIndex, Func<T[]> valuesFactory);

        protected BufferedVector<T> CreateVector(int rowIndex, int colIndex, int length, object data = null)
        {
            return new BufferedVector<T>(this, rowIndex, colIndex, length, data);
        }

        #endregion

        #region Dispose

        public void Dispose()
        {
            Dispose(true);
        }

        ~VectorBuffer()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                FreeManagedResources();
                GC.SuppressFinalize(this);
                disposed = true;
            }

            FreeUnmanagedResources();
        }

        #endregion

        #region Cleanup

        protected virtual void FreeUnmanagedResources()
        {
        }

        protected virtual void FreeManagedResources()
        {
        }

        #endregion
    }
}
