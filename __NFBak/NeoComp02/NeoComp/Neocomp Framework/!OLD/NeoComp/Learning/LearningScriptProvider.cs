using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Learning
{
    public abstract class LearningScriptProvider
    {
        public abstract bool SupportsReinitialize { get; }

        public virtual void Reinitialize()
        {
            Contract.Requires(SupportsReinitialize == true);
        }
    }
}
