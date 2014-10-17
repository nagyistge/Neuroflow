using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Learning
{
    [ContractClass(typeof(ScriptCollectionProviderContract))]
    public abstract class ScriptCollectionProvider : ScriptProvider
    {
        public abstract int Count { get; }

        public abstract Script this[int index] { get; }

        public virtual IEnumerable<Script> GetScripts(ISet<int> indexes)
        {
            Contract.Requires(indexes != null && indexes.Count > 0); 

            int count = Count;
            foreach (int index in indexes)
            {
                if (index < 0 || index >= count) throw new InvalidOperationException("Index '" + index + "' out of range.");
                yield return this[index];
            }
        }
    }

    [ContractClassFor(typeof(ScriptCollectionProvider))]
    abstract class ScriptCollectionProviderContract : ScriptCollectionProvider
    {
        public override int Count
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() > 0);
                return 0;
            }
        }

        public override Script this[int index]
        {
            get
            {
                Contract.Requires(index >= 0 && index < Count);
                Contract.Ensures(Contract.Result<Script>() != null);
                return null;
            }
        }
    }
}
