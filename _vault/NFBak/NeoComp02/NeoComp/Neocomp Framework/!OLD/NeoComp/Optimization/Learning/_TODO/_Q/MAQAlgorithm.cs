using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Optimization.Algorithms.Quantum;
using System.Diagnostics.Contracts;
using NeoComp.Networks.Computational.Neural;

namespace NeoComp.Optimization.Learning
{
    public sealed class MAQAlgorithm : BackwardLearningAlgorithm
    {
        class QuantumWeight : IQuantumStatedItem
        {
            internal QuantumWeight(INeuralConnection connection)
            {
                Contract.Requires(connection != null);

                this.connection = connection;
            }
            
            INeuralConnection connection;

            QuantumState IQuantumStatedItem.State
            {
                get { return (connection.Weight / 2.0) + 0.5; }
                set { connection.Weight = (value * 2.0) - 1.0; }
            }
        }
        
        #region Concrete Annealing Class

        sealed class Process : MSEAdaptiveQuantumAnnealing
        {
            internal Process(IEnumerable<IQuantumStatedItem> items)
                : base(items)
            {
                Contract.Requires(items != null);

                MSE = 1.0;
            }

            internal double MSE { get; set; }

            protected override double ComputeMSE()
            {
                return Math.Min(MSE * 0.5, 1.0);
            }
        }

        #endregion

        Process process;

        protected internal override void InitializeNewRun()
        {
            base.InitializeNewRun();
            process = new Process(LearningConnections.Select(c => new QuantumWeight(c.Connection)));
        }

        protected internal override void BackwardIteration(bool batch, double mse)
        {
            base.BackwardIteration(batch, mse);
            if (batch)
            {
                process.MSE = mse;
                process.Step();
            }
        }
    }
}
