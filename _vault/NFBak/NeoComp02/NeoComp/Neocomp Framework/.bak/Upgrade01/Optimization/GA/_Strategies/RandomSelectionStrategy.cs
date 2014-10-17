using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using NeoComp.Core;

namespace NeoComp.Optimization.GA
{
    public sealed class RandomSelectionStrategy : ISelectionStrategy
    {
        public IEnumerable Select(ISelectableItemCollection orderedItems, int count)
        {
            var returned = new HashSet<int>();
            int itemCount = orderedItems.Count;
            while (returned.Count != count)
            {
                int index = RandomGenerator.Random.Next(itemCount);
                if (!returned.Contains(index))
                {
                    var item = orderedItems.Select(index);
                    yield return item;
                    returned.Add(index);
                }
            }
        }
    }
}
