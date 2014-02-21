using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;

namespace NeoComp.Learning
{
    public sealed class MonteCarloBatchingStrategy : BatchingStrategy
    {
        HashSet<int> lastIndexes;

        protected override void Reinitialize()
        {
            lastIndexes = null;
        }

        protected internal override ISet<int> GetNextIndexes()
        {
            var nextIndexes = new HashSet<int>();
            int count = Batcher.BatchSize;
            int range = Batcher.ScriptProvider.Count;
            while (nextIndexes.Count != count)
            {
                int index = RandomGenerator.Random.Next(range);
                if (lastIndexes != null && lastIndexes.Contains(index)) continue;
                nextIndexes.Add(index);
            }
            return lastIndexes = nextIndexes;
        }
    }
}
