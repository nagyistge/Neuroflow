using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.Core.Vectors
{
    public abstract class VectorComputationContext : SynchronizedObject, IDisposable
    {
        #region Props and Fields

        bool disposed;

        #endregion

        #region Dispose

        public void Dispose()
        {
            Dispose(true);
        }

        ~VectorComputationContext()
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
