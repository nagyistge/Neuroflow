using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;

namespace NeoComp.Epoch
{
    public abstract class EpochBase : IEpoch, ISynchronized
    {
        #region Epoch Impl

        SyncContext syncRoot = new SyncContext();

        public SyncContext SyncRoot
        {
            get { return syncRoot; }
        }

        int currentIteration;

        public int CurrentIteration
        {
            get { lock (SyncRoot) return currentIteration; }
        }

        bool initialized;

        public bool Initialized
        {
            get { lock (SyncRoot) return initialized; }
        }

        public void Initialize()
        {
            if (!initialized)
            {
                lock (SyncRoot)
                {
                    if (!initialized)
                    {
                        DoInitialize();
                    }
                }
            }
        }

        protected virtual void DoInitialize()
        {
            initialized = true;
            currentIteration = 0;
        }

        public void Uninitialize()
        {
            if (!initialized)
            {
                lock (SyncRoot)
                {
                    if (!initialized)
                    {
                        DoInitialize();
                    }
                }
            }
        }

        protected virtual void DoUninitialize()
        {
            initialized = false;
            currentIteration = 0;
        }

        public void Step()
        {
            lock (SyncRoot)
            {
                // Ensure init:
                if (!initialized) DoInitialize();

                DoStep();
                currentIteration++;
            }
        }

        protected abstract void DoStep();

        #endregion
    }
}
