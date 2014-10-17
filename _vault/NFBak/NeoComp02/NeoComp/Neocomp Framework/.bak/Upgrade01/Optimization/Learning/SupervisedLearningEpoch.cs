using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using NeoComp.Networks.Neural;
using System.Diagnostics.Contracts;
using NeoComp.Computations;

namespace NeoComp.Optimization.Learning
{
    [ContractClass(typeof(SupervisedLearningEpochContract<>))]
    public abstract class SupervisedLearningEpoch<T> : SynchronizedObject, ISupervisedLearningEpoch
        where T : LearningStrategy
    {
        #region Constructor

        protected SupervisedLearningEpoch(
            T strategy)
        {
            Contract.Requires(strategy != null);

            Strategy = strategy;
        }

        #endregion

        #region Properties

        public abstract NeuralNetworkTest Test { get; set; }

        bool strategyIsInitialized = false;

        public T Strategy { get; private set; }

        public NeuralNetwork Network
        {
            get { return Strategy.Network; }
        }

        int iteration;

        public int Iteration
        {
            get { lock (SyncRoot) return iteration; }
        }

        #endregion

        #region Init

        public void Initialize()
        {
            EnsureStrategyInitialization();
        }

        #endregion

        #region Epoch

        public void Reset()
        {
            lock (SyncRoot)
            {
                DoReset();
            }
        }

        protected virtual void DoReset()
        {
            iteration = 0;
            Strategy.InitializeNewRun();
        }

        public NeuralNetworkTestResult Step()
        {
            EnsureStrategyInitialization();
            var result = DoStep();
            IncIteration();
            return result;
        }

        private void EnsureStrategyInitialization()
        {
            lock (SyncRoot)
            {
                if (!strategyIsInitialized)
                {
                    Strategy.InitializeNewRun();
                    strategyIsInitialized = true;
                }
            }
        }

        private void IncIteration()
        {
            lock (SyncRoot) iteration++;
        }

        protected abstract NeuralNetworkTestResult DoStep();

        #endregion

        #region ISupervisedLearningEpoch Members

        LearningStrategy ISupervisedLearningEpoch.Strategy
        {
            get { return Strategy; }
        }

        #endregion
    }

    [ContractClassFor(typeof(SupervisedLearningEpoch<>))]
    abstract class SupervisedLearningEpochContract<T> : SupervisedLearningEpoch<T>
         where T : LearningStrategy
    {
        protected SupervisedLearningEpochContract()
            : base(null)
        {
        }
        
        protected override NeuralNetworkTestResult DoStep()
        {
            Contract.Ensures(Contract.Result<NeuralNetworkTestResult>() != null);
            return null;
        }
    }
}
