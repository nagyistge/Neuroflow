using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Caching;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimizations.NeuralNetworks
{
    public sealed class CacheUnorderedNeuralVectorProvider : IUnorderedNeuralVectorsProvider
    {
        public CacheUnorderedNeuralVectorProvider(IUnorderedNeuralVectorsProvider baseProvider, ObjectCache cache = null)
        {
            Contract.Requires(baseProvider != null);

            BaseProvider = baseProvider;
            this.cache = cache ?? MemoryCache.Default;
        }

        readonly string CacheKeyPrefix = Guid.NewGuid().ToString();

        ObjectCache cache;

        HashSet<string> keys = new HashSet<string>();

        int itemsSoFar;

        public IUnorderedNeuralVectorsProvider BaseProvider { get; private set; }
        
        public int ItemCount
        {
            get { return BaseProvider.ItemCount; }
        }

        public int? ReinitalizationFrequency
        {
            get { return BaseProvider.ReinitalizationFrequency; }
        }

        public IEnumerable<NeuralVectors> GetNextVectors(IndexSet indexes)
        {
            foreach (var vector in DoGetNextVectors(indexes))
            {
                if (ReinitalizationFrequency.HasValue)
                {
                    itemsSoFar++;
                    if (itemsSoFar % ReinitalizationFrequency.Value == 0)
                    {
                        ClearCache();
                    }
                }

                yield return vector;
            }
        }

        IEnumerable<NeuralVectors> DoGetNextVectors(IndexSet indexes)
        {
            var privIndexes = new IndexSet(indexes);

            foreach (int index in privIndexes.ToList())
            {
                string key = CacheKeyPrefix + index;
                var foundVector = cache[key] as NeuralVectors;
                if (foundVector != null)
                {
                    yield return foundVector;
                    privIndexes.Remove(index);
                }
            }

            if (privIndexes.Count > 0)
            {
                var indexList = privIndexes.ToList();
                int listIndex = 0;
                foreach (var vector in BaseProvider.GetNextVectors(privIndexes))
                {
                    int vectorIndex = indexList[listIndex++];
                    string key = CacheKeyPrefix + vectorIndex;
                    cache[key] = vector;
                    keys.Add(key);
                    yield return vector;
                }
            }
        }

        private void ClearCache()
        {
            foreach (var key in keys)
            {
                cache.Remove(key);
            }
            keys.Clear();
        }
    }
}
