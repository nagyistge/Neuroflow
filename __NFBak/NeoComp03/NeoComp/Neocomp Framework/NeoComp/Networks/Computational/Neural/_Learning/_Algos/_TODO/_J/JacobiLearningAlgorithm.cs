using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Networks.Computational.Neural;
using tp = ThirdParty;

namespace NeoComp.Networks.Computational.Neural
{
    public abstract class JacobiLearningAlgorithm : BackwardLearningAlgorithm
    {
        List<List<double>> jacobi;
        
        protected int JacobiHeight
        {
            [Pure]
            get { return jacobi.Count; }
        }

        protected int JacobiWidth
        {
            [Pure]
            get { return LearningConnections.Count; }
        }

        protected double this[int row, int col]
        {
            get
            {
                // col == weights
                // row == errors

                Contract.Requires(col >= 0 && col < JacobiWidth);
                Contract.Requires(row >= 0 && row < JacobiHeight);

                return jacobi[row][col];
            }
        }

        protected tp.Matrix GetJacobiMatrix()
        {
            var j = new tp.Matrix(JacobiHeight, JacobiWidth);
            for (int row = 0; row < JacobiHeight; row++)
                for (int col = 0; col < JacobiWidth; col++)
                    j[row, col] = this[row, col];
            return j;
        }

        protected internal override void InitializeNewRun()
        {
            base.InitializeNewRun();

            jacobi = new List<List<double>>();
        }

        protected internal override void BackwardIteration(bool batch, double mse)
        {
            base.BackwardIteration(batch, mse);

            if (!batch)
            {
                var errors = new List<double>(LearningConnections.Count);
                double c = LearningConnections.Count;
                for (int idx = 0; idx < LearningConnections.Count; idx++)
                {
                    errors.Add(((IBackwardConnection)LearningConnections.ItemArray[idx].Connection).BackwardValues.Last.ErrorSum);
                }
                jacobi.Add(errors);
            }
            else
            {
                jacobi.Clear();
            }
        }
    }
}
