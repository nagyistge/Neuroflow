using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace NeoComp.Networks
{
    internal class NoAddCollection<T> : Collection<T>
    {
        internal NoAddCollection(IList<T> items)
            : base(items)
        {
            Contract.Requires(items != null);
            Contract.Requires(!items.IsReadOnly);

            created = true;
        }

        bool created;

        protected override void InsertItem(int index, T item)
        {
            if (created) throw GetCannotAddEx();
            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, T item)
        {
            if (created) throw GetCannotAddEx();
            base.SetItem(index, item);
        }

        static Exception GetCannotAddEx()
        {
            return new InvalidOperationException("Adding item to collection is not supported.");
        }
    }
}
