using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Adjustables
{
    public interface IAdjustable
    {
        IEnumerable<IAdjustableItem> FindAdjustableItems(HashSet<Guid> visitedAdjustableUIDs);
    }
}
