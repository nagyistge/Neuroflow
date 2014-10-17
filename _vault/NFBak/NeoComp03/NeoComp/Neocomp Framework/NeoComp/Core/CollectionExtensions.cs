using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Diagnostics.Contracts;

namespace NeoComp.Core
{
    public static class CollectionHelpers
    {
        [Pure]
        public static bool IsNullOrEmpty<T>(this T[] array)
        {
            return array == null || array.Length == 0;
        }

        [Pure]
        public static bool IsNullOrEmpty<T>(this ICollection<T> list)
        {
            return list == null || list.Count == 0;
        }

        [Pure]
        public static int GetLength<T>(this T[] array)
        {
            return array == null ? 0 : array.Length;
        }

        [Pure]
        public static int GetCount<T>(this ICollection<T> list)
        {
            return list == null ? 0 : list.Count;
        }
    }
}
