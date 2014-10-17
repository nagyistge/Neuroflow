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
    // TODO: Better naming on this stuff.
    public enum LearningMode : byte { Online, Batch, Parallel }

    // TODO: Put this logic to base.
    public class SupervisedAdjusterLearningEpoch<T> : SupervisedLearningEpoch<T>, ISupervisedAdjusterLearningEpoch
        where T : LearningStrategy
    {
        #region Constructor

        public SupervisedAdjusterLearningEpoch(
            T strategy, 
            LearningMode learningMode,
            NeuralNetworkTest test = null,
            int? monteCarloSelect = null,
            bool preserveTestOrder = false)
            : base(strategy)
        {
            Contract.Requires(strategy != null);
            Contract.Requires(!monteCarloSelect.HasValue || monteCarloSelect.Value > 1);

            if ((learningMode == LearningMode.Online && !(strategy is IOnlineLearningStrategy)) ||
                (learningMode == LearningMode.Batch && !(strategy is IBatchLearningStrategy)) ||
                (learningMode == LearningMode.Parallel && !(strategy is IParallelLearningStrategy)))
            {
                throw new ArgumentException("Algorihtm is not compatible with specified learning mode.", "algorithm");
            }

            LearningMode = learningMode;
            PreserveTestOrder = preserveTestOrder;
            MonteCarloSelect = monteCarloSelect;

            if (test != null)
            {
                this.test = test;
                InitializeTest();
            }
        }

        #endregion

        #region Fields

        OnlineNeuralNetworkTest onlineTest;

        #endregion

        #region Properties

        NeuralNetworkTest test;

        public override NeuralNetworkTest Test
        {
            get { lock (SyncRoot) return test; }
            set
            {
                lock (SyncRoot)
                {
                    test = value;
                    InitializeTest();
                }
            }
        }

        public LearningMode LearningMode { get; private set; }

        public bool PreserveTestOrder { get; private set; }

        public int? MonteCarloSelect { get; private set; }

        #endregion

        #region Initialize Test

        protected virtual void InitializeTest()
        {
            var test = Test;
            if (LearningMode == LearningMode.Online)
            {
                onlineTest = new OnlineNeuralNetworkTest(test, PreserveTestOrder);
            }
        }

        #endregion

        #region Epoch

        protected override NeuralNetworkTestResult DoStep()
        {
            NeuralNetworkTestResult result = null;
            lock (SyncRoot)
            {
                var test = Test;
                if (test == null) throw new InvalidOperationException("Cannot run epoch when test is not present.");

                if (LearningMode == LearningMode.Online)
                {
                    result = GetOnlineResult();
                }
                else if (LearningMode == LearningMode.Batch)
                {
                    result = GetBatchResult();
                }
                else if (LearningMode == LearningMode.Parallel)
                {
                    result = GetParallelResult();
                }
            }
            return result;
        }

        private NeuralNetworkTestResult GetParallelResult()
        {
            return ((IParallelLearningStrategy)Strategy).DoIteration(Test);
        }

        private NeuralNetworkTestResult GetBatchResult()
        {
            lock (Network.SyncRoot)
            {
                var test = Test;
                var result = test.Test(Network);
                ((IBatchLearningStrategy)Strategy).Adjust(result.MSE, result.GetErrors());
                return result;
            }
        }

        private NeuralNetworkTestResult GetOnlineResult()
        {
            return onlineTest.RunTest(Network,
                (result) => 
                {
                    ((IOnlineLearningStrategy)Strategy).Adjust(result.MSE, result.GetErrors().First(), onlineTest.Count);
                },
                true,
                MonteCarloSelect);
        }

        #endregion
    }
}
