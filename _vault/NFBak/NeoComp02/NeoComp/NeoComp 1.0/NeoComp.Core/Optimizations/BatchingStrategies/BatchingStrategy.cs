using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Collections;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimizations.BatchingStrategies
{
    [ContractClass(typeof(BatchingStrategyContract))]
    [Serializable]
    public abstract class BatchingStrategy
    {
        protected int BatchSize { get; private set; }

        protected int ItemCount { get; private set; }

        protected bool IsInitialized { get; private set; }

        public void Initialize(int itemCount, int batchSize)
        {
            Contract.Requires(itemCount > 0);
            Contract.Requires(batchSize > 0);
            Contract.Requires(itemCount >= batchSize);

            ItemCount = itemCount;
            BatchSize = batchSize;

            DoInitialize();

            IsInitialized = true;
        }

        protected abstract void DoInitialize();

        public abstract ISet<int> GetNextIndexes();
    }

    [ContractClassFor(typeof(BatchingStrategy))]
    abstract class BatchingStrategyContract : BatchingStrategy
    {
        public override ISet<int> GetNextIndexes()
        {
            Contract.Requires(IsInitialized);
            Contract.Ensures(Contract.Result<ISet<int>>() != null);
            Contract.Ensures(Contract.Result<ISet<int>>().Count > 0);
            Contract.Ensures(Contract.ForAll(Contract.Result<ISet<int>>(), (index) => index >= 0 && index < ItemCount));
            return null;
        }
    }
}
