using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Caching;
using System.Diagnostics.Contracts;

namespace Neuroflow.Activities
{
    [Serializable]
    internal sealed class SerializableCache : IDisposable
    {
        internal SerializableCache(string name)
        {
            Contract.Requires(!String.IsNullOrEmpty(name));

            this.name = name;
        }

        string name;

        [NonSerialized]
        MemoryCache cache;

        internal MemoryCache Cache
        {
            get
            {
                if (cache == null)
                {
                    cache = new MemoryCache(name);
                }
                return cache;
            }
        }

        public void Dispose()
        {
            if (cache != null)
            {
                cache.Dispose();
                cache = null;
                GC.SuppressFinalize(this);
            }
        }
    }
}
