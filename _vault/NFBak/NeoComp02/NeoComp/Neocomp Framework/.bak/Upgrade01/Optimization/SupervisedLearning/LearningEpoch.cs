using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using NeoComp.Epoch;
using System.Diagnostics;
using NeoComp.Networks2.Computational.Neural;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.SupervisedLearning
{
    [ContractClass(typeof(LearningEpochContract))]
    public abstract class LearningEpoch : SynchronizedObject, IEpoch
    {
        protected LearningEpoch(NeuralNetworkTest test = null)
        {
            if ((this.currentTest = test) != null) TestChanged();
        }

        NeuralNetworkTest currentTest;

        public NeuralNetworkTest CurrentTest
        {
            get { lock (SyncRoot)return currentTest; }
            set
            {
                Contract.Requires(value != null);
                lock (SyncRoot)
                {
                    currentTest = value;
                    TestChanged();
                }
            }
        }

        Stopwatch sw = new Stopwatch();

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

        public TimeSpan ElapsedTime
        {
            get { lock (SyncRoot) return sw.Elapsed; }
        }

        LearningTestResult bestResult;

        public LearningTestResult BestResult
        {
            get { lock (SyncRoot) return bestResult; }
        }

        LearningTestResult currentResult;

        public LearningTestResult CurrentResult
        {
            get { lock (SyncRoot) return currentResult; }
        }

        public void Initialize()
        {
            InitializeInternal();
        }

        public void Uninitialize()
        {
            UninitializeInternal();
        }

        public void Step()
        {
            lock (SyncRoot)
            {
                if (this.currentTest == null) throw new InvalidOperationException("Cannot start epoch. Test is null.");
                if (!initialized)
                {
                    sw.Start();
                    try
                    {
                        InitializeInternal();
                    }
                    finally
                    {
                        sw.Stop();
                    }
                }

                sw.Start();
                try
                {
                    var testResult = Step(this.currentTest);
                    currentResult = new LearningTestResult(this.currentTest, testResult);
                    if (bestResult == null || testResult.MSE < bestResult.MSE)
                    {
                        bestResult = new LearningTestResult(this.currentTest, testResult);
                    }
                }
                finally
                {
                    sw.Stop();
                }
                currentIteration++;
            }
        }
        
        internal virtual void InitializeInternal()
        {
            lock (SyncRoot)
            {
                if (!initialized)
                {
                    InitializeNewRun();
                    sw.Reset();
                    currentIteration = 0;
                    initialized = true;
                    bestResult = currentResult = null;
                }
            }
        }

        internal virtual void UninitializeInternal()
        {
            lock (SyncRoot)
            {
                if (initialized)
                {
                    UninitializeData();
                    sw.Reset();
                    currentIteration = 0;
                    initialized = false;
                    bestResult = currentResult = null;
                }
            }
        }

        protected virtual void TestChanged() { }

        protected abstract void InitializeNewRun();

        protected abstract void UninitializeData();

        protected abstract NeuralNetworkTestResult Step(NeuralNetworkTest test);
    }

    [ContractClassFor(typeof(LearningEpoch))]
    abstract class LearningEpochContract : LearningEpoch
    {
        protected override NeuralNetworkTestResult Step(NeuralNetworkTest test)
        {
            Contract.Ensures(Contract.Result<NeuralNetworkTestResult>() != null);
            return null;
        }
    }
}
