using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neuroflow
{
    public abstract class DisposableObject : IDisposable
    {
        bool disposed;
        
        public void Dispose()
        {
            CallDispose(true);
        }

        ~DisposableObject()
        {
            CallDispose(false);
        }

        private void CallDispose(bool disposing)
        {
            if (!disposed) Dispose(disposing);
        }

        protected virtual void Dispose(bool disposing)
        {
            disposed = true;

            if (disposing)
            {
                CleanupManagedResources();
                GC.SuppressFinalize(this);
            }

            CleanupNativeResources();
        }

        protected virtual void CleanupManagedResources()
        {
        }

        protected virtual void CleanupNativeResources()
        {
        }
    }
}
