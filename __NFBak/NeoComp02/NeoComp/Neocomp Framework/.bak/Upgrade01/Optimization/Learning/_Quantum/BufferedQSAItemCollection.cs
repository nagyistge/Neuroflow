using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using NeoComp.Networks;
using System.Diagnostics.Contracts;
using NeoComp.Core;
using NeoComp.Adjustables;

namespace NeoComp.Optimization.Learning
{
    internal sealed class BufferedQSAItemCollection : ReadOnlyCollection<BufferedQSAItem>
    {
        internal BufferedQSAItemCollection(IEnumerable<IAdjustableItem> items)
            : base(items.Select(i => new BufferedQSAItem(i)).ToArray())
        {
            Contract.Requires(items != null);
        }

        internal BufferedQSAItem[] ItemArray
        {
            get { return (BufferedQSAItem[])Items; }
        }

        internal void Apply()
        {
            foreach (var item in ItemArray) item.Apply();
        }

        internal void Import()
        {
            foreach (var item in ItemArray) item.Import();
        }
    }
}
