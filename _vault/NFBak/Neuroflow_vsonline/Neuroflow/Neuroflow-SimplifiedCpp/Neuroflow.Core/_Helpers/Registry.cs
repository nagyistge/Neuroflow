using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics.Contracts;

namespace Neuroflow.Core
{
    public class Registry<TKey, TValue>
    {
        ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();

        Dictionary<TKey, TValue> values = new Dictionary<TKey, TValue>();

        public TValue GetOrCreate(TKey key, Func<TValue> factoryMethod)
        {
            Contract.Requires(factoryMethod != null);

            rwLock.EnterUpgradeableReadLock();
            try
            {
                TValue result;
                if (values.TryGetValue(key, out result)) return result;

                rwLock.EnterWriteLock();
                try
                {
                    values.Add(key, result = factoryMethod());
                    return result;
                }
                finally
                {
                    rwLock.ExitWriteLock();
                }
            }
            finally
            {
                rwLock.ExitUpgradeableReadLock();
            }
        }
    }
}
