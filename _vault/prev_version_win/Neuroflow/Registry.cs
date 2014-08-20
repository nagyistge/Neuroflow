using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Neuroflow
{
    public class Registry<TKey, TValue>
    {
        ReaderWriterLockSlim locker = new ReaderWriterLockSlim();

        Dictionary<TKey, TValue> data = new Dictionary<TKey, TValue>();

        public IEnumerable<TKey> Keys
        {
            get { return data.Keys; }
        }

        public IEnumerable<TValue> Values
        {
            get { return data.Values; }
        }

        public TValue GetOrCreate(TKey key, Func<TValue> factoryMethod)
        {
            locker.EnterUpgradeableReadLock();
            try
            {
                TValue result;
                if (data.TryGetValue(key, out result)) return result;

                result = factoryMethod();
                locker.EnterWriteLock();
                try
                {
                    data.Add(key, result);
                    return result;
                }
                finally
                {
                    locker.ExitWriteLock();
                }
            }
            finally
            {
                locker.ExitUpgradeableReadLock();
            }
        }

        public void Register(TKey key, TValue value)
        {
            locker.EnterWriteLock();
            try
            {
                data[key] = value;
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        public bool TryGet(TKey key, out TValue value)
        {
            locker.EnterReadLock();
            try
            {
                return data.TryGetValue(key, out value);
            }
            finally
            {
                locker.ExitReadLock();
            }
        }

        public void Remove(TKey key)
        {
            locker.EnterReadLock();
            try
            {
                data.Remove(key);
            }
            finally
            {
                locker.ExitReadLock();
            }
        }
    }

}
