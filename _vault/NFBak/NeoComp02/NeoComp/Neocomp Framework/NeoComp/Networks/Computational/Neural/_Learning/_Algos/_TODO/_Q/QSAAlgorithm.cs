using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Optimization.Algorithms.Quantum;
using NeoComp.Networks.Computational.Neural;
using System.Diagnostics.Contracts;

namespace NeoComp.Networks.Computational.Neural
{
    public sealed class QSAAlgorithm : GlobalOptimizationLearningAlgorithm
    {
        const double qsaStrength = 0.15;
        const double qsaStabilize = 0.95;
        const double qsaDissolve = 0.0001;

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

        QuantumStabilizerAlgorithm qsa;

        double lastMSE;

        protected internal override void InitializeNewRun(AlgoInitializationMode mode)
        {
            base.InitializeNewRun(mode);

            if (mode == AlgoInitializationMode.Startup)
            {
                qsa = new QuantumStabilizerAlgorithm(LearningConnections.Select(c => new QuantumWeight(c.Connection)), qsaStrength, false);
                lastMSE = double.MaxValue;
            }
        }

        protected internal override void BackwardIteration(bool batch, double mse)
        {
            base.BackwardIteration(batch, mse);

            if (batch)
            {
                QSAStep();
            }
        }

        private void QSAStep()
        {
            if (CurrentMSE <= lastMSE)
            {
                qsa.Stabilize(qsaStabilize);
            }
            else if (CurrentMSE > lastMSE)
            {
                qsa.Dissolve(qsaDissolve);
            }
            lastMSE = CurrentMSE;
        }
    }
}
