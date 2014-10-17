using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Core;

namespace NeoComp.Learning
{
    [ContractClass(typeof(ScriptBatcherContract))]
    public abstract class ScriptBatcher : SynchronizedObject
    {
        public ScriptBatcher(ScriptProvider scriptProvider, int batchSize, int? reinitializeFrequency = null)
        {
            Contract.Requires(scriptProvider != null);
            Contract.Requires(batchSize > 0);
            Contract.Requires(reinitializeFrequency == null || reinitializeFrequency.Value > 0);

            BatchSize = batchSize;
            ScriptProvider = scriptProvider;
            ReinitializeFrequency = reinitializeFrequency;
        }

        int counter = 0;

        bool initialized;
        
        public virtual int BatchSize { get; private set; }

        public int? ReinitializeFrequency { get; private set; }

        public ScriptProvider ScriptProvider { get; private set; }
        
        public ScriptBatch GetNext()
        {
            lock (SyncRoot)
            {
                using (new SyncObjectGuard(ScriptProvider))
                {
                    if (!initialized) throw new InvalidOperationException(this + " is not initialized.");

                    if (ReinitializeFrequency.HasValue)
                    {
                        if (++counter % ReinitializeFrequency.Value == 0) Reinitialize();
                    }
                    return GetNextBatch();
                }
            }
        }

        public void Initialize()
        {
            lock (SyncRoot)
            {
                using (new SyncObjectGuard(ScriptProvider))
                {
                    if (!initialized)
                    {
                        Reinitialize();
                        initialized = true;
                    }
                }
            }
        }

        protected internal virtual void Reinitialize()
        {
            ScriptProvider.Reinitialize();
        }

        protected abstract ScriptBatch GetNextBatch();
    }

    [ContractClassFor(typeof(ScriptBatcher))]
    abstract class ScriptBatcherContract : ScriptBatcher
    {
        protected ScriptBatcherContract()
            : base(null, 0)
        {
        }
        
        protected override ScriptBatch GetNextBatch()
        {
            Contract.Ensures(Contract.Result<ScriptBatch>() != null);
            Contract.Ensures(Contract.Result<ScriptBatch>().Count > 0 && Contract.Result<ScriptBatch>().Count <= BatchSize);
            return null;
        }
    }
}
