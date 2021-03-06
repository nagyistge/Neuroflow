﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.Learning
{
    public abstract class BatchingStrategy
    {
        protected LearningScriptCollectionBatcher Batcher { get; private set; }

        internal void Reinitialize(LearningScriptCollectionBatcher batcher)
        {
            if (Batcher == null) Batcher = batcher;
            if (Batcher != null && Batcher != batcher) throw new InvalidOperationException(this + " is used by " + Batcher + " already.");
            Reinitialize();
        }

        protected abstract void Reinitialize();

        protected internal abstract ISet<int> GetNextIndexes();
    }
}
