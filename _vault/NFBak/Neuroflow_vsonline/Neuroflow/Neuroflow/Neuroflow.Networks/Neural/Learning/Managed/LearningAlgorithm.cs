﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Neuroflow.Networks.Neural.Managed;
using Neuroflow.Core.Vectors;

namespace Neuroflow.Networks.Neural.Learning.Managed
{
    public abstract class LearningAlgorithm
    {
        public LearningRule Rule { get; private set; }

        protected ConnectedLayer[] ConnectedLayers { get; private set; }

        protected bool RunParallel { get; private set; }

        internal void InitializeAlgo(BufferAllocator allocator, LearningRule rule, ConnectedLayer[] connectedLayers, ManagedNNInitParameters initPars)
        {
            Contract.Requires(rule != null);
            Contract.Requires(connectedLayers != null);
            Contract.Requires(connectedLayers.Length > 0);
            Contract.Requires(initPars != null);

            Rule = rule;
            ConnectedLayers = connectedLayers;
            RunParallel = initPars.RunParallel;
            Ininitalize(allocator);
        }

        protected virtual void Ininitalize(BufferAllocator allocator) { }

        unsafe protected internal virtual void InitializeNewRun(float* valueBuffer) { }

        unsafe protected internal virtual void ForwardIteration(float* valueBuffer, bool isNewBatch) { }

        unsafe protected internal virtual void BackwardIteration(float* valueBuffer, double averageError, bool isBatchIteration, int batchSize = 0) { }
    }
}
