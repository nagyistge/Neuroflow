using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Learning
{
    [ContractClass(typeof(LearningScriptCollectionProviderContract))]
    public abstract class LearningScriptCollectionProvider : LearningScriptProvider
    {
        public abstract int Count { get; }

        public abstract LearningScript this[int index] { get; }

        public virtual IEnumerable<LearningScript> GetScripts(ISet<int> indexes)
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

    [ContractClassFor(typeof(LearningScriptCollectionProvider))]
    abstract class LearningScriptCollectionProviderContract : LearningScriptCollectionProvider
    {
        public override int Count
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() > 0);
                return 0;
            }
        }

        public override LearningScript this[int index]
        {
            get
            {
                Contract.Requires(index >= 0 && index < Count);
                Contract.Ensures(Contract.Result<LearningScript>() != null);
                return null;
            }
        }
    }
}
