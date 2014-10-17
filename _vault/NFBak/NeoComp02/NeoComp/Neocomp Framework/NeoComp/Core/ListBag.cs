using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Runtime.Serialization;
using System.Diagnostics.Contracts;
using NeoComp.Optimization.Algorithms.Selection;

namespace NeoComp.Core
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = NeoComp.xmlns, Name = "listBag")]
    public class ListBag<TKey, TValue> : IList<TValue>
        where TKey : IComparable<TKey>
    {
        #region Static

        static readonly RuntimeUIDGenerator uidGen = new RuntimeUIDGenerator(true);

        static ListBag() { } 

        #endregion

        #region Key Struct

        struct Key : IComparable<Key>, IEquatable<Key>
        {
            public Key(TKey value)
            {
                this.value = value;
                uid = uidGen.Next();
            }

            public TKey value;

            uint uid;

            public int CompareTo(Key other)
            {
                int c = value.CompareTo(other.value);
                if (c == 0) c = uid.CompareTo(other.uid);
                return c;
            }

            public bool Equals(Key other)
            {
                return uid == other.uid;
            }

            public override bool Equals(object obj)
            {
                return obj is Key ? Equals((Key)obj) : false;
            }

            public override int GetHashCode()
            {
                return uid.GetHashCode();
            }

            public override string ToString()
            {
                return uid.ToString();
            }

            public static implicit operator Key(TKey key)
            {
                return new Key(key);
            }
        }

        #endregion

        #region Constructors

        public ListBag()
        {
            list = new SortedList<Key, TValue>();
        }

        public ListBag(int capacity)
        {
            Contract.Requires(capacity >= 0);
            
            list = new SortedList<Key, TValue>(capacity);
        }

        public ListBag(IList<TValue> list, Func<TValue, TKey> keySelector)
        {
            Contract.Requires(list != null);
            Contract.Requires(keySelector != null);

            this.list = new SortedList<Key, TValue>(list.Count);
            foreach (var item in list) this.list.Add(keySelector(item), item);
        }

        public ListBag(IDictionary<TKey, TValue> dict)
        {
            Contract.Requires(dict != null);

            list = new SortedList<Key, TValue>(dict.Count);
            foreach (var kvp in dict) list.Add(kvp.Key, kvp.Value);
        }

        #endregion

        #region Fields

        [DataMember(Name = "values")]
        SortedList<Key, TValue> list;

        #endregion

        #region Management

        public void Add(TKey key, TValue value)
        {
            list.Add(key, value);
        }

        public IEnumerable<TValue> Select(ISelectionAlgorithm selectionAlgo, int count)
        {
            Contract.Requires(selectionAlgo != null);
            Contract.Requires(count >= 0 && count <= Count);

            foreach (int index in selectionAlgo.Select(IntRange.CreateExclusive(0, Count), count))
            {
                yield return list.Values[index];
            }
        }

        #endregion

        #region IList<TValue> Members

        static NotSupportedException GetNotSupportedEx(string opName)
        {
            return new NotSupportedException(opName + " is not supported.");
        }

        public int IndexOf(TValue item)
        {
            return list.Values.IndexOf(item);
        }

        public void Insert(int index, TValue item)
        {
            throw GetNotSupportedEx("Insert");
        }

        public void RemoveAt(int index)
        {
            list.RemoveAt(index);
        }

        public TValue this[int index]
        {
            get
            {
                return list.Values[index];
            }
            set
            {
                throw GetNotSupportedEx("Set Item[index]");
            }
        }

        #endregion

        #region ICollection<TValue> Members

        public void Add(TValue item)
        {
            throw GetNotSupportedEx("Add");
        }

        public void Clear()
        {
            list.Clear();
        }

        public bool Contains(TValue item)
        {
            return list.Values.Contains(item);
        }

        public void CopyTo(TValue[] array, int arrayIndex)
        {
            list.Values.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return list.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(TValue item)
        {
            throw GetNotSupportedEx("Remove");
        }

        #endregion

        #region IEnumerable<TValue> Members

        public IEnumerator<TValue> GetEnumerator()
        {
            return list.Values.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
