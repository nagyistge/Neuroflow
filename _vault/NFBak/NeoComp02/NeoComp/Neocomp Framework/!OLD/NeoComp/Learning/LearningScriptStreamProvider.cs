using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Learning
{
    public abstract class LearningScriptStreamProvider : LearningScriptProvider
    {
        public abstract LearningScript GetNext();
    }

    abstract class LearningScriptStreamProviderContract : LearningScriptStreamProvider
    {
        public override LearningScript GetNext()
        {
            Contract.Ensures(SupportsReinitialize || Contract.Result<LearningScript>() != null);
            return null;
        }     }
}
