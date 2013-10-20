using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow
{
    public static class Marshaled
    {
        public static T Instance<T>(this Marshaled<T> marshaled) where T : class
        {
            if (marshaled == null) return null;
            return marshaled.ManagedObject;
        }
    }

    public class Marshaled<T> : DisposableObject where T : class
    {
        public Marshaled(T managedObject)
        {
            ManagedObject = managedObject;
        }

        public T ManagedObject { get; set; }

        public IDisposable NativeVersion { get; set; }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (NativeVersion != null)
            {
                NativeVersion.Dispose();
                NativeVersion = null;
            }
        }
    }
}
