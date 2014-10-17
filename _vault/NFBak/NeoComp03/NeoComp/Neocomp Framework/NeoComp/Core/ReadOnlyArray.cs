using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;

namespace NeoComp.Core
{
    public static class ReadOnlyArray
    {
        public static ReadOnlyArray<T> Wrap<T>(params T[] items)
        {
            return new ReadOnlyArray<T>(items, true);
        }
    }
    
    [Serializable]
    [DataContract(IsReference = true, Namespace = NeoComp.xmlns, Name = "roArray")]
    public class ReadOnlyArray<T> : IList<T>
    {
        #region Constructors

        public ReadOnlyArray(params T[] items)
            : this(items, false)
        {
        }

        protected internal ReadOnlyArray(T[] items, bool wrap)
        {
            ItemArray = items == null ? new T[0] : (wrap ? items : items.ToArray());
        }

        public ReadOnlyArray(IEnumerable<T> items)
        {
            Contract.Requires(items != null);

            ItemArray = items.ToArray();
        }

        public ReadOnlyArray(IList<T> items)
        {
            Contract.Requires(items != null);

            ItemArray = items.ToArray();
        } 

        #endregion

        #region Properties

        [DataMember(Name = "items")]
        protected internal T[] ItemArray { get; private set; }

        private IList<T> Items
        {
            get { return (IList<T>)ItemArray; }
        }

        public bool IsEmpty
        {
            [Pure]
            get { return ItemArray.Length == 0; }
        } 

        #endregion

        #region ToString

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var value in ItemArray)
            {
                if (sb.Length != 0) sb.Append(", ");
                if (value == null) sb.Append("null"); else sb.Append(value);
            }
            return sb.ToString();
        }

        #endregion

        #region IList<T> Members

        NotSupportedException GetReadOnlyEx()
        {
            throw new NotSupportedException(this + " is read only.");
        }

        public int IndexOf(T item)
        {
            return Items.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            throw GetReadOnlyEx();
        }

        public void RemoveAt(int index)
        {
            throw GetReadOnlyEx();
        }

        public T this[int index]
        {
            get
            {
                return Items[index];
            }
            set
            {
                throw GetReadOnlyEx();
            }
        }

        #endregion

        #region ICollection<T> Members

        public void Add(T item)
        {
            throw GetReadOnlyEx();
        }

        public void Clear()
        {
            throw GetReadOnlyEx();
        }

        public bool Contains(T item)
        {
            return Items.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Items.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return Items.Count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public bool Remove(T item)
        {
            throw GetReadOnlyEx();
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        #endregion
    }
}
