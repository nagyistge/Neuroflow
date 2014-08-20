using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Computations;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;

namespace NeoComp.Learning
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = NeoComp.xmlns, Name = "script")]
    public sealed class Script : ComputationScript<ScriptEntry, double>
    {
        #region Constructors

        public Script(ScriptEntry entry)
            : base(entry)
        {
            Contract.Requires(entry != null);
        }

        public Script(IList<ScriptEntry> entryList)
            : base(entryList)
        {
            Contract.Requires(entryList != null && entryList.Count > 0);
        }

        public Script(IEnumerable<ScriptEntry> entryColl)
            : base(entryColl.ToList())
        {
            Contract.Requires(entryColl != null);
        } 

        #endregion
    }
}
