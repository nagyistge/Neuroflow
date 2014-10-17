using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Neuroflow.Core.Vectors.BatchingStrategies
{
    [ContractClass(typeof(BatchingStrategyContract))]
    [Serializable]
    public abstract class BatchingStrategy
    {
        protected int BatchSize { get; private set; }

        protected int ItemCount { get; private set; }

        public void Initialize(int itemCount, int batchSize)
        {
            Contract.Requires(itemCount > 0);
            Contract.Requires(batchSize > 0);

            ItemCount = itemCount;
            BatchSize = Math.Min(batchSize, itemCount);

            DoInitialize();
        }

        protected abstract void DoInitialize();

        public ISet<int> GetNextIndexes()
        {
            if (BatchSize == ItemCount)
            {
                return new HashSet<int>(Enumerable.Range(0, ItemCount).OrderByRandom());
            }
            else
            {
                return DoGetNextIndexes();
            }
        }

        protected abstract ISet<int> DoGetNextIndexes();
    }

    [ContractClassFor(typeof(BatchingStrategy))]
    abstract class BatchingStrategyContract : BatchingStrategy
    {
        protected override ISet<int> DoGetNextIndexes()
        {
            Contract.Ensures(Contract.Result<ISet<int>>() != null);
            Contract.Ensures(Contract.Result<ISet<int>>().Count > 0);
            Contract.Ensures(Contract.ForAll(Contract.Result<ISet<int>>(), (index) => index >= 0 && index < ItemCount));
            return null;
        }
    }
}
