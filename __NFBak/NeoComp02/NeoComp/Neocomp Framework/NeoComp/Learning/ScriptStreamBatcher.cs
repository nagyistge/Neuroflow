using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Learning
{
    public sealed class ScriptStreamBatcher : ScriptBatcher
    {
        public ScriptStreamBatcher(ScriptStreamProvider scriptProvider, int batchSize, int? reinitializeFrequency = null)
            : base(scriptProvider, batchSize, reinitializeFrequency)
        {
            Contract.Requires(batchSize > 0);
            Contract.Requires(scriptProvider != null);
            Contract.Requires(reinitializeFrequency == null || reinitializeFrequency.Value > 0);
        }

        new public ScriptStreamProvider ScriptProvider
        {
            get { return (ScriptStreamProvider)base.ScriptProvider; }
        }

        protected override ScriptBatch GetNextBatch()
        {
            var scripts = new List<Script>(BatchSize);
            for (int idx = 0; idx < BatchSize; idx++)
            {
                var next = ScriptProvider.GetNext();
                if (next == null)
                {
                    ScriptProvider.Reinitialize();
                    next = ScriptProvider.GetNext();
                    if (next == null)
                    {
                        throw new InvalidOperationException("Null script arrived after stream reinitialization.");
                    }
                }
                scripts.Add(next);
            }
            return new ScriptBatch(scripts);
        }
    }
}
