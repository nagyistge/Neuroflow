using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Computations2;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;

namespace NeoComp.Learning
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = NeoComp.xmlns, Name = "learningScript")]
    public sealed class LearningScript : ComputationScript<LearningScriptEntry, double>
    {
        #region Constructors

        public LearningScript(LearningScriptEntry entry)
            : base(entry)
        {
            Contract.Requires(entry != null);
        }

        public LearningScript(IList<LearningScriptEntry> entryList)
            : base(entryList)
        {
            Contract.Requires(entryList != null && entryList.Count > 0);
        }

        public LearningScript(IEnumerable<LearningScriptEntry> entryColl)
            : base(entryColl.ToList())
        {
            Contract.Requires(entryColl != null);
        } 

        #endregion
    }
}
