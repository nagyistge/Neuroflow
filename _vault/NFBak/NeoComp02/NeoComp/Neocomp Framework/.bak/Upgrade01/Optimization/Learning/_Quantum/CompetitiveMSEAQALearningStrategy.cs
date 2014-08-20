using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks;
using NeoComp.Networks.Neural;
using System.Diagnostics.Contracts;
using NeoComp.Optimization.Quantum;
using System.Diagnostics;

namespace NeoComp.Optimization.Learning
{
    public sealed class CompetitiveMSEAQALearningStrategy : LearningStrategy, IParallelLearningStrategy
    {
        #region Annealing Process Class

        sealed class Process : MSEAdaptiveQuantumAnnealing
        {
            internal Process(CompetitiveMSEAQALearningStrategy strategy, BufferedQSAItemCollection items)
                : base(items)
            {
                Contract.Requires(strategy != null);
                Contract.Requires(items != null);

                this.strategy = strategy;
                this.items = items;
            }

            CompetitiveMSEAQALearningStrategy strategy;

            BufferedQSAItemCollection items;

            internal NeuralNetworkTestResult LastResult { get; private set; }

            protected override double ComputeMSE()
            {
                Debug.Assert(strategy.CurrentTest != null);
                lock (strategy.Network.SyncRoot)
                {
                    items.Apply();
                    LastResult = strategy.CurrentTest.Test(strategy.Network);
                    return LastResult.MSE;
                }
            }
        } 

        #endregion

        #region Constructor

        public CompetitiveMSEAQALearningStrategy(NeuralNetwork network, int competitorCount = 10)
            : base(network)
        {
            Contract.Requires(network != null);
            Contract.Requires(competitorCount > 0);

            this.competitorCount = competitorCount;
        } 

        #endregion

        #region Fields and Properties

        int competitorCount;

        CompetitiveQuantumAnnealing<Process> compo;

        internal NeuralNetworkTest CurrentTest { get; private set; }

        #endregion

        #region Init

        protected internal override void InitializeNewRun()
        {
            if (compo == null)
            {
                compo = new CompetitiveQuantumAnnealing<Process>(
                    Enumerable.Range(0, competitorCount)
                              .Select(idx => new Process(this, new BufferedQSAItemCollection(Network.GetAdjustableItems()))));
            }
            else
            {
                compo.Reset();
            }
        }

        #endregion

        #region Iteration

        NeuralNetworkTestResult IParallelLearningStrategy.DoIteration(NeuralNetworkTest test)
        {
            CurrentTest = test;
            compo.Step();
            return compo.BestProcess.LastResult;
        } 

        #endregion
    }
}
