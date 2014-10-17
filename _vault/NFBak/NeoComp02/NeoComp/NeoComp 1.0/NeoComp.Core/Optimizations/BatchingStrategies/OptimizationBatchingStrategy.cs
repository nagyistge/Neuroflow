using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimizations.BatchingStrategies
{
    [Serializable]
    [ContractClass(typeof(OptimizationBatchingStrategyContract))]
    public abstract class OptimizationBatchingStrategy : BatchingStrategy
    {
        public abstract void SetLastResult(BatchExecutionResult lastResult);
    }

    [ContractClassFor(typeof(OptimizationBatchingStrategy))]
    abstract class OptimizationBatchingStrategyContract : OptimizationBatchingStrategy
    {
        public override void SetLastResult(BatchExecutionResult lastResult)
        {
            Contract.Requires(IsInitialized);
            Contract.Requires(!lastResult.IsEmpty);
        }
    }
}
