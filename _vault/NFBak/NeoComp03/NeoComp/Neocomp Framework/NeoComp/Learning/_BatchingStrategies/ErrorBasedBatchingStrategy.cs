using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Learning
{
    [ContractClass(typeof(ErrorBasedBatchingStrategyContract))]
    public abstract class ErrorBasedBatchingStrategy : BatchingStrategy, ILearningErrorReport
    {
        protected abstract void ErrorReportArrived(ScriptBatchExecutionResult result);
        
        #region ILearningErrorReport Members

        void ILearningErrorReport.ReportError(ScriptBatchExecutionResult result)
        {
            if (!result.IsEmpty && result.Errors.Length == Batcher.BatchSize)
            {
                ErrorReportArrived(result);
            }
        }

        #endregion
    }

    [ContractClassFor(typeof(ErrorBasedBatchingStrategy))]
    abstract class ErrorBasedBatchingStrategyContract : ErrorBasedBatchingStrategy
    {
        protected override void ErrorReportArrived(ScriptBatchExecutionResult result)
        {
            Contract.Requires(!result.IsEmpty && result.Errors.Length == Batcher.BatchSize);
        }
    }
}
