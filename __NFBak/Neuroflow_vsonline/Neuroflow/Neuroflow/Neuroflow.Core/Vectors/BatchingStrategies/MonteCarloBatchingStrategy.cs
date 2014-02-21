using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neuroflow.Core.Vectors.BatchingStrategies
{
    [Serializable]
    public sealed class MonteCarloBatchingStrategy : BatchingStrategy
    {
        HashSet<int> lastIndexes;

        protected override void DoInitialize()
        {
            lastIndexes = null;
        }

        protected override ISet<int> DoGetNextIndexes()
        {
            var nextIndexes = new HashSet<int>();
            int count = BatchSize;
            int range = ItemCount;
            while (nextIndexes.Count != count)
            {
                int index = RandomGenerator.Random.Next(range);
                if (ItemCount >= BatchSize * 2)
                {
                    if (lastIndexes != null && lastIndexes.Contains(index)) continue;
                }
                nextIndexes.Add(index);
            }
            return lastIndexes = nextIndexes;
        }
    }
}
