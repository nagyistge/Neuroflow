using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Learning
{
    [ContractClass(typeof(ILearningErrorReportContract))]
    public interface ILearningErrorReport
    {
        void ReportError(ScriptBatchExecutionResult result);
    }

    [ContractClassFor(typeof(ILearningErrorReport))]
    class ILearningErrorReportContract : ILearningErrorReport
    {
        #region ILearningErrorReport Members

        void ILearningErrorReport.ReportError(ScriptBatchExecutionResult result)
        {
            Contract.Requires(!result.IsEmpty);
        }

        #endregion
    }
}
