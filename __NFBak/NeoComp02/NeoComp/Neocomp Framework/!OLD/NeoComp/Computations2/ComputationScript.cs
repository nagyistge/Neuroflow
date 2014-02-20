using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace NeoComp.Computations2
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = NeoComp.xmlns, Name = "compScript")]
    public abstract class ComputationScript<TEntry, T> : IList<TEntry>
        where T : struct
        where TEntry : ComputationScriptEntry<T>
    {
        protected ComputationScript(TEntry entry)
        {
            Contract.Requires(entry != null);

            entries = new TEntry[1];
            entries[0] = entry;
        }
        
        protected ComputationScript(IList<TEntry> entryList)
        {
            Contract.Requires(entryList != null && entryList.Count > 0);

            entries = entryList.ToArray();
        }

        protected ComputationScript(IEnumerable<TEntry> entryColl)
        {
            Contract.Requires(entryColl != null);

            entries = entryColl.ToArray();

            if (Entries.Count == 0) throw new InvalidOperationException("Entry collection is empty.");
        }

        [DataMember(Name = "entries")]
        TEntry[] entries;

        private IList<TEntry> Entries
        {
            get { return entries; }
        }

        #region IList<TEntry> Members

        static NotSupportedException GetReadOnlyEx()
        {
            throw new NotSupportedException("ComputationScript is read only.");
        }

        public int IndexOf(TEntry item)
        {
            return Entries.IndexOf(item);
        }

        public void Insert(int index, TEntry item)
        {
            throw GetReadOnlyEx();
        }

        public void RemoveAt(int index)
        {
            throw GetReadOnlyEx();
        }

        public TEntry this[int index]
        {
            get
            {
                return Entries[index];
            }
            set
            {
                throw GetReadOnlyEx();
            }
        }

        #endregion

        #region ICollection<TEntry> Members

        public void Add(TEntry item)
        {
            throw GetReadOnlyEx();
        }

        public void Clear()
        {
            throw GetReadOnlyEx();
        }

        public bool Contains(TEntry item)
        {
            return Entries.Contains(item);
        }

        public void CopyTo(TEntry[] array, int arrayIndex)
        {
            Entries.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return Entries.Count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public bool Remove(TEntry item)
        {
            throw GetReadOnlyEx();
        }

        #endregion

        #region IEnumerable<TEntry> Members

        public IEnumerator<TEntry> GetEnumerator()
        {
            return Entries.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Entries.GetEnumerator();
        }

        #endregion
    }
}
