using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using NeoComp.Core;

namespace NeoComp.Computations
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = NeoComp.xmlns, Name = "compScript")]
    public abstract class ComputationScript<TEntry, T> : ReadOnlyArray<TEntry>
        where T : struct
        where TEntry : ComputationScriptEntry<T>
    {
        protected ComputationScript(TEntry entry)
            : base(entry)
        {
            Contract.Requires(entry != null);
        }
        
        protected ComputationScript(IList<TEntry> entryList)
            : base(entryList)
        {
            Contract.Requires(entryList != null && entryList.Count > 0);
        }

        protected ComputationScript(IEnumerable<TEntry> entryColl)
            : base(entryColl)
        {
            Contract.Requires(entryColl != null);
            
            if (ItemArray.Length == 0) throw new InvalidOperationException("Entry collection is empty.");
        }
    }
}
