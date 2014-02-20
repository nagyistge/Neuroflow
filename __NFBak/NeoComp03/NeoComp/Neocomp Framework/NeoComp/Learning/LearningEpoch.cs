using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Epoch;
using NeoComp.Core;
using System.Diagnostics.Contracts;

namespace NeoComp.Learning
{
    public class LearningEpoch : SynchronizedObject, IEpoch
    {
        #region Constructor

        public LearningEpoch(ScriptBatchExecution learningExecution, ScriptBatcher batcher)
        {
            Contract.Requires(learningExecution != null);
            Contract.Requires(batcher != null);
            
            LearningExecution = learningExecution;
            Batcher = batcher;
            CreateResults();
        }

        public LearningEpoch(ScriptBatchExecution learningExecution, ScriptBatcher batcher, ScriptBatcher validationBatcher, int validationFrequency = 10)
        {
            Contract.Requires(learningExecution != null);
            Contract.Requires(batcher != null);
            Contract.Requires(validationBatcher != null);
            Contract.Requires(validationFrequency > 0);
            Contract.Requires(batcher != validationBatcher);

            LearningExecution = learningExecution;
            Batcher = batcher;
            ValidationFrequency = validationFrequency;
            ValidationBatcher = validationBatcher;
            CreateResults();
        }

        private void CreateResults()
        {
            CurrentResult = new LearningResult(this);
            CurrentValidationResult = new LearningResult(this);
            BestResult = new LearningResult(this, true);
            BestValidationResult = new LearningResult(this, true);
        }

        #endregion

        #region Properties and Fields

        int counter = 0;

        ScriptBatchExecution validationExec;

        public ScriptBatchExecution LearningExecution { get; private set; }

        public ScriptBatcher Batcher { get; private set; }

        public ScriptBatcher ValidationBatcher { get; private set; }

        public int ValidationFrequency { get; private set; }

        bool initialized;

        public bool Initialized
        {
            get { lock (SyncRoot) return initialized; }
        }

        int currentIteration;

        public int CurrentIteration
        {
            get { lock (SyncRoot) return currentIteration; }
        }

        public LearningResult CurrentResult { get; private set; }

        public LearningResult BestResult { get; private set; }

        public LearningResult CurrentValidationResult { get; private set; }

        public LearningResult BestValidationResult { get; private set; }

        #endregion

        #region IInitializable Members

        public void Initialize()
        {
            lock (SyncRoot)
            {
                EnsureInitialization();
            }
        }

        private void EnsureInitialization()
        {
            if (!initialized)
            {
                DoInitialization();
                initialized = true;
            }
        }

        protected virtual void DoInitialization()
        {
            currentIteration = 0;
            counter = 0;
            Batcher.Initialize();
            if (ValidationBatcher != null)
            {
                validationExec = new ScriptBatchExecution(LearningExecution.ComputationUnit, LearningExecution.ErrorScale);
            }
        }

        public void Uninitialize()
        {
            lock (SyncRoot)
            {
                if (initialized)
                {
                    DoUninitialization();
                }
            }
        }

        protected virtual void DoUninitialization()
        {
            currentIteration = 0;
            validationExec = null;
        }

        #endregion

        #region IEpoch Members

        public void Step()
        {
            lock (SyncRoot)
            {
                EnsureInitialization();

                var nextBatch = Batcher.GetNext();
                var result = LearningExecution.Execute(nextBatch, IterationCallback);
                nextBatch.ReportError(result);
            }
        }

        private void IterationCallback(ScriptBatchExecutionResult result)
        {
            CurrentResult.Update(result.AverageError);
            BestResult.Update(result.AverageError);
            
            if (ValidationBatcher != null && (ValidationFrequency == 1 || ++counter % ValidationFrequency == 0))
            {
                var nextVBatch = ValidationBatcher.GetNext();
                var vresult = validationExec.Execute(nextVBatch);
                CurrentValidationResult.Update(vresult.AverageError);
                BestValidationResult.Update(vresult.AverageError);
            }

            currentIteration++;
        }

        #endregion
    }
}
