using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using tp = ThirdParty;
using NeoComp.Networks.Computational.Neural;
using System.Diagnostics;

namespace NeoComp.Networks.Computational.Neural
{
    public sealed class LMAlgorithm : JacobiLearningAlgorithm
    {
        #region Fields

        double lambda, delta;

        Action next = null;

        double[] lastWeights;

        double lastError, error;

        #endregion

        #region Init

        protected internal override void InitializeNewRun()
        {
            base.InitializeNewRun();

            //newton = new Newton(this);

            var rules = LearningConnections.Select(c => ((LMRule)c.Rule));
            lambda = 0.0;
            delta = 0.0;
            foreach (var rule in rules)
            {
                lambda += rule.Lambda;
                delta += rule.Delta;
            }
            double count = LearningConnections.Count;
            lambda /= count;
            delta /= count;

            error = lastError = 1.0;
            lastWeights = LearningConnections.Select(c => c.Connection.Weight).ToArray();

            next = null;
        }

        #endregion

        #region LM

        protected internal override void BackwardIteration(bool batch, double mse)
        {
            if (batch)
            {
                error = mse;
                if (next == null) LM(); else next();
            }
            base.BackwardIteration(batch, mse);
        }

        private void LM()
        {
            Action step = null;

            // Step by Hessian:
            step = () =>
            {
                Debug.WriteLine(lambda);

                SaveWeightsAndError();
                
                var jacobi = GetJacobiMatrix();
                var jacobiT = GetJacobiMatrix().Transpose();
                var jTj = jacobiT * jacobi;

                var matrix2 = new tp.Matrix(jTj.Rows, jTj.Rows);
                for (int j = 0; j < matrix2.Rows; j++) matrix2[j, j] = lambda;

                var matrix3 = jTj + matrix2;

                bool inv = true;
                tp.Matrix hessInv = new ThirdParty.Matrix();
                try
                {
                    hessInv = matrix3.Inverse();
                }
                catch
                {
                    inv = false;
                }

                var a = LearningConnections.ItemArray;
                for (int idx = 0; idx < a.Length; idx++)
                {
                    var lc = a[idx];
                    var bc = (IBackwardConnection)lc.Connection;
                    var rule = (LMRule)lc.Rule;
                    double gradient = bc.BackwardValues.AvgGradient;
                    double nv = 0.0;
                    int ccount = inv ? hessInv.Columns : 0;
                    double vDelta = 0.0;

                    if (inv)
                    {
                        for (int col = 0; col < ccount; col++)
                        {
                            nv += hessInv[idx, col];
                        }

                        vDelta = nv * gradient;
                    }
                    else
                    {
                        //bc.Weight += gradient * 0.01;
                        //next = step;
                        return;
                    }

                    if (vDelta < -5.0) vDelta = -5.0; else if (vDelta > 5.0) vDelta = 5.0;
                    bc.Weight += vDelta;
                }

                CalculateNewError(() =>
                {
                    double derror = error - lastError;
                    if (error <= lastError || Math.Abs(derror) < 0.00000)
                    {
                        // dec lambda
                        lambda /= delta;
                        if (lambda < double.Epsilon) lambda = double.Epsilon;

                        //Console.WriteLine("OK");

                        // GOTO 10
                        step();
                        return;
                    }
                    else
                    {
                        RestoreWeights();

                        //if (error > 0.01)
                        //{
                        //    var ac = LearningConnections.ItemArray;
                        //    for (int idx = 0; idx < ac.Length; idx++)
                        //    {
                        //        var lc = ac[idx];
                        //        var bc = (IBackwardConnection)lc.Connection;
                        //        bc.Weight += bc.BackwardValues.AvgGradient * 0.01;
                        //    }
                        //}

                        lambda *= delta;
                        if (lambda > 100) lambda = 100;

                        // Retry:
                        next = step;
                    }                    
                });
            };

            step();
        }

        private void CalculateNewError(Action after)
        {
            next = after;
        }

        private void SaveWeightsAndError()
        {
            lastError = error;
            var a = LearningConnections.ItemArray;
            for (int idx = 0; idx < a.Length; idx++)
            {
                lastWeights[idx] = a[idx].Connection.Weight;
            }
        }

        private void RestoreWeights()
        {
            var a = LearningConnections.ItemArray;
            for (int idx = 0; idx < a.Length; idx++)
            {
                a[idx].Connection.Weight = lastWeights[idx];
            }
        }

        public static bool NaNInf(double d)
        {
            if (!double.IsNaN(d))
            {
                return double.IsInfinity(d);
            }
            return true;
        }

        #endregion
    }
}
