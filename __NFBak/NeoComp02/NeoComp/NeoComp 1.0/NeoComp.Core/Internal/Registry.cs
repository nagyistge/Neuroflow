using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Internal
{
    internal static class Registry
    {
        internal static TValue GetOrRegister<TKey, TValue>(this IDictionary<TKey, TValue> registry, TKey key, Func<TValue> valueFactoryMethod)
        {
            Contract.Requires(registry != null);
            Contract.Requires(valueFactoryMethod != null);

            TValue result;
            lock (registry)
            {
                if (!registry.TryGetValue(key, out result))
                {
                    result = valueFactoryMethod();
                    registry.Add(key, result);
                }
                return result;
            }
        }
    }
}
