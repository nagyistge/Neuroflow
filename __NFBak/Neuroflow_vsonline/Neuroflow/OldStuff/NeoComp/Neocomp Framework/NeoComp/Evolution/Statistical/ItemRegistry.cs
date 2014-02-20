using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using NeoComp.Core;

namespace NeoComp.Evolution.Statistical
{
    internal delegate T CreateRandomValueExceptMethod<T>(HashSet<T> items);
    
    internal sealed class ItemRegistry<T>
    {
        internal ItemRegistry(int resolution, IEnumerable<T> items)
        {
            Contract.Requires(resolution > 1);
            Contract.Requires(items != null);

            Resolution = resolution;
            Build(items);
        }

        internal List<ItemCount<T>> ItemCounts { get; private set; }

        internal int Resolution { get; private set; }
        
        internal int AllItemCount { get; private set; }

        internal int RegisteredItemCount { get; private set; }

        internal int RemainingItemCount { get; private set; }

        private void Build(IEnumerable<T> items)
        {
            AllItemCount = 0;
            var itemCountDict = new Dictionary<T, ItemCount<T>>();
            foreach (var item in items)
            {
                ItemCount<T> itemCount;
                if (itemCountDict.TryGetValue(item, out itemCount))
                {
                    itemCount.Inc();
                }
                else
                {
                    itemCountDict.Add(item, new ItemCount<T>(item));
                }
                AllItemCount++;
            }

            var itemCountList = itemCountDict.Values.ToList();
            if (itemCountList.Count > Resolution)
            {
                itemCountList.Sort();
            }

            ItemCounts = new List<ItemCount<T>>(Math.Min(itemCountList.Count, Resolution));
            for (int idx = 0; idx < itemCountList.Count && idx < Resolution; idx++)
            {
                var current = itemCountList[idx];
                ItemCounts.Add(current);
                RegisteredItemCount += current.Count;
            }
            RemainingItemCount = AllItemCount - RegisteredItemCount;
        }

        internal T Pick(CreateRandomValueExceptMethod<T> rndMethod)
        {
            Contract.Requires(rndMethod != null);

            int rnd = RandomGenerator.Random.Next(AllItemCount);
            if (rnd < RegisteredItemCount)
            {
                int itemIndex = 0;
                for (int idx = 0; idx < ItemCounts.Count - 1; idx++)
                {
                    var current = ItemCounts[idx];
                    itemIndex += current.Count;
                    var next = ItemCounts[idx + 1];
                    if (itemIndex + next.Count > rnd)
                    {
                        return current.Item;
                    }
                }
                return ItemCounts[ItemCounts.Count - 1].Item;
            }
            else
            {
                var except = new HashSet<T>(ItemCounts.Select(c => c.Item));
                return rndMethod(except);
            }
        }
    }
}
