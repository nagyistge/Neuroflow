using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.Evolution.Statistical
{
    internal sealed class ItemCount<T> : IComparable<ItemCount<T>>
    {
        internal ItemCount(T item)
        {
            Item = item;
            Count = 1;
        }

        internal T Item { get; private set; }

        internal int Count { get; private set; }

        internal void Inc()
        {
            Count++;
        }

        #region IComparable<ItemCount<T>> Members

        int IComparable<ItemCount<T>>.CompareTo(ItemCount<T> other)
        {
            return -Count.CompareTo(other.Count);
        }

        #endregion
    }
}
