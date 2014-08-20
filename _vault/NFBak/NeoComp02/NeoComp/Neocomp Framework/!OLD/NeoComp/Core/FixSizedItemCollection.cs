using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace NeoComp.Core
{
    public abstract class FixSizedItemCollection<T> : Collection<T>
    {
        protected FixSizedItemCollection(params T[] entries)
        {
            if (!entries.IsNullOrEmpty()) Initialize(entries);
        }

        protected FixSizedItemCollection(IEnumerable<T> entries)
        {
            Contract.Requires(entries != null);

            Initialize(entries);
        }

        public bool IsEmpty
        {
            [Pure]
            get { return Count == 0; }
        }

        protected override void InsertItem(int index, T item)
        {
            Validate(item);
            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, T item)
        {
            Validate(item); 
            base.SetItem(index, item);
        }

        protected virtual void Validate(T item)
        {
            if (!IsEmpty && !SizeEquals(item, this[0]))
            {
                var ex = new InvalidOperationException("Added item's size is invalid. See provided data 'item' for details.");
                ex.Data["item"] = item;
                throw ex;
            }
        }

        protected abstract bool SizeEquals(T item1, T item2);

        void Initialize(IEnumerable<T> entries)
        {
            foreach (var entry in entries) Add(entry);
        }
    }
}
