using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks;
using NeoComp.Networks.Neural;
using NeoComp.Optimization.Quantum;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.Learning
{
    public sealed class MSEAQALearningStrategy : LearningStrategy, IParallelLearningStrategy
    {
        #region Concrete Annealing Class

        sealed class Process : MSEAdaptiveQuantumAnnealing
        {
            internal Process(MSEAQALearningStrategy strategy)
                : base(strategy.Network.GetAdjustableItems().Select(i => new QSAItem(i)))
            {
                Contract.Requires(strategy != null);

                this.strategy = strategy;
            }

            MSEAQALearningStrategy strategy;

            internal NeuralNetworkTestResult LastResult { get; private set; }

            protected override double ComputeMSE()
            {
                LastResult = strategy.CurrentTest.Test(strategy.Network);
                return LastResult.MSE;
            }
        } 

        #endregion

        #region Constructor

        public MSEAQALearningStrategy(NeuralNetwork network)
            : base(network)
        {
        } 

        #endregion

        #region Fields and Properties

        Process process;

        NeuralNetworkTestResult last;

        internal NeuralNetworkTest CurrentTest { get; private set; }

        #endregion

        #region Init and Step

        protected internal override void InitializeNewRun()
        {
            if (process == null) process = new Process(this); else process.Reset();
            CurrentTest = null;
            last = null;
        }

        NeuralNetworkTestResult IParallelLearningStrategy.DoIteration(NeuralNetworkTest test)
        {
            CurrentTest = test;
            bool improved = process.Step();
            if (last == null || improved)
            {
                last = process.LastResult;
            }
            return last;
        }

        #endregion
    }
}
