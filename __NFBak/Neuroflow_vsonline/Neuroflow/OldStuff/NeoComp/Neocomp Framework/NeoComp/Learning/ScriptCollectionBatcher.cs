using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Core;
using NeoComp.Caching;

namespace NeoComp.Learning
{
    public sealed class ScriptCollectionBatcher : ScriptBatcher
    {
        public ScriptCollectionBatcher(BatchingStrategy strategy, ScriptCollectionProvider scriptProvider, int batchSize, int? reinitializeFrequency = null)
            : base(scriptProvider, batchSize, reinitializeFrequency)
        {
            Contract.Requires(scriptProvider != null);
            Contract.Requires(batchSize > 0);
            Contract.Requires(reinitializeFrequency == null || reinitializeFrequency.Value > 0);

            Strategy = strategy;
        }

        public BatchingStrategy Strategy { get; private set; }

        new public ScriptCollectionProvider ScriptProvider
        {
            get { return (ScriptCollectionProvider)base.ScriptProvider; }
        }

        public override int BatchSize
        {
            get
            {
                int size = base.BatchSize;
                int max = ScriptProvider.Count;
                if (size > max) size = max;
                return size;
            }
        }

        ScriptBatch lastBatch = null;

        string uid = Guid.NewGuid().ToString();

        protected internal override void Reinitialize()
        {
            base.Reinitialize();
            lastBatch = null;
            Strategy.Reinitialize(this);
            ClearCache();
            uid = Guid.NewGuid().ToString();
        }

        private void ClearCache()
        {
            var cache = ScriptCache.Cache;
            if (cache != null)
            {
                for (int idx = 0; idx < ScriptProvider.Count; idx++)
                {
                    string key = idx.ToString() + uid;
                    cache.Remove(key);
                }
            }
        }

        protected override ScriptBatch GetNextBatch()
        {
            var indexes = Strategy.GetNextIndexes();
            if (indexes.IsNullOrEmpty())
            {
                if (lastBatch == null)
                {
                    throw new InvalidOperationException(Strategy + " has returned null or empty index collection.");
                }
            }
            else
            {
                var cache = ScriptCache.Cache;
                if (cache != null)
                {
                    lastBatch = GetNextBatchCached(indexes, cache);
                }
                else
                {
                    lastBatch = GetNextBatchUncached(indexes);
                }
            }
            return lastBatch;
        }

        ScriptBatch GetNextBatchCached(ISet<int> indexes, ICache cache)
        {
            var remainingIndexes = new HashSet<int>();
            var scripts = new Script[indexes.Count];
            int sidx = 0;
            foreach (int index in indexes)
            {
                string key = index.ToString() + uid;
                var script = cache[key] as Script;
                if (script == null)
                {
                    remainingIndexes.Add(index);
                }
                else
                {
                    scripts[sidx++] = script;
                }
            }
            if (remainingIndexes.Count != 0)
            {
                if (remainingIndexes.Count == 1)
                {
                    int index = remainingIndexes.First();
                    var script = ScriptProvider[index];
                    scripts[sidx++] = script;
                    string key = index.ToString() + uid;
                    cache.Add(key, script);
                }
                else
                {
                    var indexEnum = remainingIndexes.GetEnumerator();
                    foreach (var script in ScriptProvider.GetScripts(remainingIndexes))
                    {
                        indexEnum.MoveNext();
                        int index = indexEnum.Current;
                        scripts[sidx++] = script;
                        string key = index.ToString() + uid;
                        cache.Add(key, script);
                    }
                }
            }
            return new ScriptBatch(scripts, Strategy as ILearningErrorReport);
        }

        ScriptBatch GetNextBatchUncached(ISet<int> indexes)
        {
            return new ScriptBatch(ScriptProvider.GetScripts(indexes), Strategy as ILearningErrorReport); 
        }
    }
}
