using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks2.Computational.Neural;
using System.Diagnostics.Contracts;
using NeoComp.Optimization.Quantum;

namespace NeoComp.Optimization.SupervisedLearning
{
    public sealed class MAQLearningEpoch : QuantumAdjusterLearningEpoch
    {
        #region Concrete Annealing Class

        sealed class Process : MSEAdaptiveQuantumAnnealing
        {
            internal Process(MAQLearningEpoch owner, IEnumerable<IQuantumStatedItem> items)
                : base(items)
            {
                Contract.Requires(owner != null);

                this.owner = owner;
            }

            MAQLearningEpoch owner;

            internal NeuralNetworkTestResult LastResult { get; private set; }

            protected override double ComputeMSE()
            {
                LastResult = owner.CurrentTest.Test(owner.Network);
                return LastResult.MSE;
            }
        }

        #endregion

        #region Constructor

        public MAQLearningEpoch(NeuralNetwork network, NeuralNetworkTest test = null)
            : base(network, test)
        {
            Contract.Requires(network != null);
        } 

        #endregion

        Process process;

        protected override void Initialize(IEnumerable<IQuantumStatedItem> items)
        {
            if (process == null) process = new Process(this, items); else process.Reset();
        }

        protected override void UninitializeData()
        {
            process = null;
        }

        protected override NeuralNetworkTestResult Step(NeuralNetworkTest test)
        {
            process.Step();
            return process.LastResult;
        }
    }
}
