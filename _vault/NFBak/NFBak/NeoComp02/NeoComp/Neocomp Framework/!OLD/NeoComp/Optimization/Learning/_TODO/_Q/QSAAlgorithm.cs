using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Optimization.Algorithms.Quantum;
using NeoComp.Networks.Computational.Neural;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.Learning
{
    public sealed class QSAAlgorithm : BackwardLearningAlgorithm
    {
        const double qsaStrength = 0.2;
        const double qsaStabilize = 0.95;
        const double qsaDissolve = 0.01;

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

        protected internal override void InitializeNewRun()
        {
            base.InitializeNewRun();

            qsa = new QuantumStabilizerAlgorithm(LearningConnections.Select(c => new QuantumWeight(c.Connection)), qsaStrength, false);
            lastMSE = 1.0;
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
                qsa.Dissolve(qsaDissolve * CurrentMSE);
            }
            lastMSE = CurrentMSE;
        }
    }
}
