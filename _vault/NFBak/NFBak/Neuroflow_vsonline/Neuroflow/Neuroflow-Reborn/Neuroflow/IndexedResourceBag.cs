using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow
{
    internal abstract class IndexedResourceBag<T> : DisposableObject
    {
        internal IndexedResourceBag(Func<T> factoryMethod)
        {
            Debug.Assert(factoryMethod != null);

            this.factoryMethod = factoryMethod;
        }

        Func<T> factoryMethod;

        List<T> resources = new List<T>();

        internal int Count
        {
            get { return resources.Count; }
        }

        internal T this[int index]
        {
            get
            {
                resources.EnsureSize(index + 1, factoryMethod);
                return resources[index];
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (resources != null)
            {
                ResourceManager.Free(resources);
                resources = null;
            }
        }
    }
}
