using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace NeoComp.Learning
{
    public sealed class LearningScriptBatch : ReadOnlyCollection<LearningScript>
    {
        public LearningScriptBatch(IList<LearningScript> scriptList, ILearningErrorReport errorReport = null)
            : base(scriptList)
        {
            Contract.Requires(scriptList != null && scriptList.Count > 0);

            this.errorReport = errorReport;
        }

        public LearningScriptBatch(IEnumerable<LearningScript> scriptCollection, ILearningErrorReport errorReport = null)
            : base(scriptCollection.ToList())
        {
            Contract.Requires(scriptCollection != null);

            if (base.Items.Count == 0) throw new InvalidOperationException("Vector Computation Script collection is empty.");

            this.errorReport = errorReport;
        }

        ILearningErrorReport errorReport;

        internal void ReportError(ScriptBatchExecutionResult result)
        {
            Contract.Requires(!result.IsEmpty);

            if (errorReport != null) errorReport.ReportError(result);
        }
    }
}
