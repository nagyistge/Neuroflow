using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorTest
{
    public class SmartComparer<T> : IComparer<T>, ICloneable where T : IEquatable<T>
    {
        struct Pair : IEquatable<Pair>
        {
            internal T Item1 { get; set; }

            internal T Item2 { get; set; }

            public bool Equals(Pair other)
            {
                return Item1.Equals(other.Item1) && Item1.Equals(other.Item1);
            }
        }

        public SmartComparer(IComparer<T> baseComparer)
        {
            Contract.Requires(baseComparer != null);

            this.BaseComparer = baseComparer;
        }

        object sync = new object();

        public IComparer<T> BaseComparer { get; private set; }

        Dictionary<Pair, int> results;

        public int Compare(T x, T y)
        {
            lock (sync)
            {
                if (results == null) Reset();

                var key = new Pair { Item1 = x, Item2 = y };
                int result;

                if (results.TryGetValue(key, out result)) return result;

                var rKey = new Pair { Item1 = y, Item2 = x };

                if (results.TryGetValue(rKey, out result)) return -result;

                result = BaseComparer.Compare(x, y);
                results.Add(key, result);
                return result;
            }
        }

        public void Reset()
        {
            lock (sync)
            {
                results = new Dictionary<Pair, int>();
            }
        }

        public object Clone()
        {
            var bcc = this.BaseComparer as ICloneable;
            return new SmartComparer<T>(bcc == null ? this.BaseComparer : (IComparer<T>)bcc.Clone());
        }
    }
}
