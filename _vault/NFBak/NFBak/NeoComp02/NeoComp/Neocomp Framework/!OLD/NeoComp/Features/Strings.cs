using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace NeoComp.Features
{
    [DataContract(IsReference = true, Namespace = NeoComp.xmlns, Name = "featureIDs")]
    [Serializable]
    public sealed class Strings : IList<string>
    {
        #region Constructors

        public Strings(params string[] strings)
        {
            Contract.Requires(strings != null);
            Contract.Requires(strings.Length > 0);

            StrArray = ToArray(strings);
        }

        public Strings(IEnumerable<string> strings)
        {
            Contract.Requires(strings != null);

            StrArray = ToArray(strings);
        }

        private static string[] ToArray(IEnumerable<string> strings)
        {
            var list = new List<string>();
            foreach (var id in strings.Distinct())
            {
                if (string.IsNullOrEmpty(id)) throw new ArgumentException("One of strings is null or empty.", "strings");
                list.Add(id);
            }
            if (list.Count == 0) throw new ArgumentException("String collection is empty.", "strings");
            return list.ToArray();
        }

        #endregion

        #region Properties

        [DataMember(Name = "values")]
        internal string[] StrArray { get; private set; }

        private IList<string> StrList
        {
            get { return (IList<string>)StrArray; }
        }

        #endregion

        #region Operators

        public static implicit operator Strings(string[] stringArray)
        {
            Contract.Requires(stringArray != null);
            Contract.Requires(stringArray.Length > 0);

            return new Strings(stringArray);
        }

        public static implicit operator Strings(string str)
        {
            Contract.Requires(!string.IsNullOrEmpty(str));

            return new Strings(new[] { str });
        }

        #endregion

        #region IList

        private static Exception GetROEx()
        {
            return new NotSupportedException("FeatureIDCollection is read only.");
        }

        public int IndexOf(string item)
        {
            return StrList.IndexOf(item);
        }

        public void Insert(int index, string item)
        {
            throw GetROEx();
        }

        public void RemoveAt(int index)
        {
            throw GetROEx();
        }

        public string this[int index]
        {
            get
            {
                return StrArray[index];
            }
            set
            {
                throw GetROEx();
            }
        }

        public void Add(string item)
        {
            throw GetROEx();
        }

        public void Clear()
        {
            throw GetROEx();
        }

        public bool Contains(string item)
        {
            return StrList.Contains(item);
        }

        public void CopyTo(string[] array, int arrayIndex)
        {
            StrList.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return StrArray.Length; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public bool Remove(string item)
        {
            throw GetROEx();
        }

        public IEnumerator<string> GetEnumerator()
        {
            return StrList.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        } 

        #endregion
    }
}
