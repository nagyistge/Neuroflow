using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using NeoComp.Core;
using System.Runtime.Serialization;

namespace NeoComp.Learning
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = NeoComp.xmlns, Name = "scriptBatch")]
    public sealed class ScriptBatch : ReadOnlyArray<Script>
    {
        public ScriptBatch(IList<Script> scriptList, ILearningErrorReport errorReport = null)
            : base(scriptList)
        {
            Contract.Requires(scriptList != null && scriptList.Count > 0);

            this.errorReport = errorReport;
        }

        public ScriptBatch(IEnumerable<Script> scriptCollection, ILearningErrorReport errorReport = null)
            : base(scriptCollection.ToList())
        {
            Contract.Requires(scriptCollection != null);

            if (ItemArray.Length == 0) throw new InvalidOperationException("Vector Computation Script collection is empty.");

            this.errorReport = errorReport;
        }

        internal ScriptBatch(Script[] scripts, ILearningErrorReport errorReport = null)
            : base(scripts, true)
        {
            Contract.Requires(scripts != null && scripts.Length > 0);

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
