using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Learning
{
    [ContractClass(typeof(LearningScriptBatcherContract))]
    public abstract class LearningScriptBatcher
    {
        public LearningScriptBatcher(LearningScriptProvider scriptProvider, int batchSize, int? reinitializeFrequency = null)
        {
            Contract.Requires(scriptProvider != null);
            Contract.Requires(batchSize > 0);
            Contract.Requires(reinitializeFrequency == null || (reinitializeFrequency.Value > 0 && scriptProvider.SupportsReinitialize));

            BatchSize = batchSize;
            ScriptProvider = scriptProvider;
            ReinitializeFrequency = reinitializeFrequency;
        }

        int counter = 0;

        bool initialized;
        
        public virtual int BatchSize { get; private set; }

        public int? ReinitializeFrequency { get; private set; }

        public LearningScriptProvider ScriptProvider { get; private set; }
        
        public LearningScriptBatch GetNext()
        {
            if (!initialized) throw new InvalidOperationException(this + " is not initialized."); 
            
            if (ReinitializeFrequency.HasValue)
            {
                if (counter++ % ReinitializeFrequency.Value == 0) Reinitialize();
            }
            return GetNextBatch();
        }

        public void Initialize()
        {
            if (!initialized) Reinitialize();
        }

        protected internal virtual void Reinitialize()
        {
            if (ScriptProvider.SupportsReinitialize) ScriptProvider.Reinitialize();
            initialized = true;
        }

        protected abstract LearningScriptBatch GetNextBatch();
    }

    [ContractClassFor(typeof(LearningScriptBatcher))]
    abstract class LearningScriptBatcherContract : LearningScriptBatcher
    {
        protected LearningScriptBatcherContract()
            : base(null, 0)
        {
        }
        
        protected override LearningScriptBatch GetNextBatch()
        {
            Contract.Ensures(Contract.Result<LearningScriptBatch>() != null);
            Contract.Ensures(Contract.Result<LearningScriptBatch>().Count > 0 && Contract.Result<LearningScriptBatch>().Count <= BatchSize);
            return null;
        }
    }
}
