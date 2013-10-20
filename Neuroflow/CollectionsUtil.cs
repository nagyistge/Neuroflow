using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow
{
    internal static class CollectionsUtil
    {
        internal static List<T> EnsureSize<T>(this List<T> list, int size, Func<T> valueFactory = null)
        {
            Debug.Assert(list != null);
            Debug.Assert(size >= 0);

            int count = list.Count;
            if (count < size)
            {
                int capacity = list.Capacity;
                if (capacity < size)
                    list.Capacity = Math.Max(size, capacity * 2);

                while (count < size)
                {
                    if (valueFactory != null) list.Add(valueFactory()); else list.Add(default(T));
                    ++count;
                }
            }

            return list;
        }
    }
}
