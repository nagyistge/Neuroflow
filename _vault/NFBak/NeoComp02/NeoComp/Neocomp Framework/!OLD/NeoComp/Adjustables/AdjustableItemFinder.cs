using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Diagnostics.Contracts;

namespace NeoComp.Adjustables
{
    public struct AdjustableItemFinder
    {
        public AdjustableItemFinder(IEnumerable items)
        {
            Contract.Requires(items != null);

            this.items = items;
        }

        IEnumerable items;

        public IEnumerable<IAdjustableItem> FindAll(HashSet<Guid> visitedAdjustableUIDs)
        {
            if (visitedAdjustableUIDs == null) visitedAdjustableUIDs = new HashSet<Guid>();
            foreach (object obj in items)
            {
                var ai = obj as IAdjustableItem;
                if (ai != null) yield return ai;
                var a = obj as IAdjustable;
                if (a != null)
                {
                    var i = a as IIdentifiedAdjustable;
                    if (i != null && visitedAdjustableUIDs.Contains(i.UID)) continue;
                    foreach (var subai in a.FindAdjustableItems(visitedAdjustableUIDs)) yield return subai;
                    if (i != null) visitedAdjustableUIDs.Add(i.UID);
                }
            }
        }
    }
}
