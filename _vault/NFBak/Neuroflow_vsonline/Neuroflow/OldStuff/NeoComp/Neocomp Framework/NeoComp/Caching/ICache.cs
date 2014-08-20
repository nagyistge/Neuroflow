using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Caching
{
    [ContractClass(typeof(ICacheContract))]
    public interface ICache
    {
        object this[string key] { get; }

        void Add(string key, object obj);

        void Remove(string key);
    }

    [ContractClassFor(typeof(ICache))]
    class ICacheContract : ICache
    {
        object ICache.this[string key]
        {
            get
            {
                Contract.Requires(!String.IsNullOrEmpty(key));
                return null;
            }
        }

        void ICache.Add(string key, object obj)
        {
            Contract.Requires(!String.IsNullOrEmpty(key));
            Contract.Requires(obj != null);
        }

        void ICache.Remove(string key)
        {
            Contract.Requires(!String.IsNullOrEmpty(key));
        }
    }
}
