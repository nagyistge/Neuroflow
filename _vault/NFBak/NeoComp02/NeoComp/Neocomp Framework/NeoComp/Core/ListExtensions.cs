using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace NeoComp.Core
{
    public static class ListExtensions
    {
        public static void AddRange(this IList list, IEnumerable items)
        {
            Args.AreNotNull(list, "list", items, "items");
            foreach (var item in items) list.Add(item);
        }

        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            Args.AreNotNull(collection, "list", items, "items");
            foreach (var item in items) collection.Add(item);
        }
    }
}
