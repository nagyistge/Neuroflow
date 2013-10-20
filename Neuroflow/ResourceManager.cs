using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow
{
    internal static class ResourceManager
    {
        internal static void Free(object obj)
        {
            if (obj == null) return;
            if (obj is IDisposable) ((IDisposable)obj).Dispose();
        }

        internal static void Free(IDisposable obj)
        {
            if (obj == null) return;
            obj.Dispose();
        }

        internal static void Free(IEnumerable collection)
        {
            if (collection == null) return;
            foreach (var obj in collection) Free(obj);
        }

        internal static void Free(IEnumerable<IDisposable> collection)
        {
            if (collection == null) return;
            foreach (var obj in collection) Free(obj);
        }
    }
}
