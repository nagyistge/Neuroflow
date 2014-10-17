using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Core;
using NeoComp.Caching;

namespace NeoComp.Learning
{
    public sealed class LearningScriptCollectionBatcher : LearningScriptBatcher
    {
        public LearningScriptCollectionBatcher(BatchingStrategy strategy, LearningScriptCollectionProvider scriptProvider, int batchSize, int? reinitializeFrequency = null)
            : base(scriptProvider, batchSize, reinitializeFrequency)
        {
            Contract.Requires(scriptProvider != null);
            Contract.Requires(batchSize > 0);
            Contract.Requires(reinitializeFrequency == null || reinitializeFrequency.Value > 0);

            Strategy = strategy;
        }

        public BatchingStrategy Strategy { get; private set; }

        new public LearningScriptCollectionProvider ScriptProvider
        {
            get { return (LearningScriptCollectionProvider)base.ScriptProvider; }
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

        LearningScriptBatch lastBatch = null;

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
            var cache = LearningScriptCache.Cache;
            if (cache != null)
            {
                for (int idx = 0; idx < ScriptProvider.Count; idx++)
                {
                    string key = idx.ToString() + uid;
                    cache.Remove(key);
                }
            }
        }

        protected override LearningScriptBatch GetNextBatch()
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
                var cache = LearningScriptCache.Cache;
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

        LearningScriptBatch GetNextBatchCached(ISet<int> indexes, ICache cache)
        {
            // TODO: Grouping.
            var scripts = new List<LearningScript>(indexes.Count);
            foreach (int index in indexes)
            {
                string key = index.ToString() + uid;
                var script = cache[key] as LearningScript;
                if (script == null)
                {
                    script = ScriptProvider[index];
                    cache.Add(key, script);
                }
                scripts.Add(script);
            }
            return new LearningScriptBatch(scripts, Strategy as ILearningErrorReport);
        }

        LearningScriptBatch GetNextBatchUncached(ISet<int> indexes)
        {
            return new LearningScriptBatch(ScriptProvider.GetScripts(indexes), Strategy as ILearningErrorReport); 
        }
    }
}
