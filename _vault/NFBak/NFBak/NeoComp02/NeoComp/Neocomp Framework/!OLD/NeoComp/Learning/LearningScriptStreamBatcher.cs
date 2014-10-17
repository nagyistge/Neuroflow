using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Learning
{
    public sealed class LearningScriptStreamBatcher : LearningScriptBatcher
    {
        public LearningScriptStreamBatcher(LearningScriptStreamProvider scriptProvider, int batchSize, int? reinitializeFrequency = null)
            : base(scriptProvider, batchSize, reinitializeFrequency)
        {
            Contract.Requires(batchSize > 0);
            Contract.Requires(scriptProvider != null);
            Contract.Requires(reinitializeFrequency == null || (reinitializeFrequency.Value > 0 && scriptProvider.SupportsReinitialize));
        }

        new public LearningScriptStreamProvider ScriptProvider
        {
            get { return (LearningScriptStreamProvider)base.ScriptProvider; }
        }

        protected override LearningScriptBatch GetNextBatch()
        {
            var scripts = new List<LearningScript>(BatchSize);
            for (int idx = 0; idx < BatchSize; idx++)
            {
                var next = ScriptProvider.GetNext();
                if (next == null)
                {
                    if (!ScriptProvider.SupportsReinitialize)
                    {
                        throw new InvalidOperationException("End of stream reached, but script provider doesn't suppoert reinitialize.");
                    }
                    ScriptProvider.Reinitialize();
                    next = ScriptProvider.GetNext();
                    if (next == null)
                    {
                        throw new InvalidOperationException("Null script arrived after stream reinitialization.");
                    }
                }
                scripts.Add(next);
            }
            return new LearningScriptBatch(scripts);
        }
    }
}
