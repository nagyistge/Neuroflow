using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks.Computational.Neural;
using NeoComp.Core;
using System.Diagnostics;

namespace NeoComp.Networks.Computational.Neural
{
    public sealed class SCGAlgorithm : BackwardLearningAlgorithm
    {
        #region Constants

        const double StartSigma = 0.0001;

        const double StartLambda = 0.000001;

        const double MaxLambda = double.MaxValue / 100;

        const double Tolerance = 0.00000001;

        const double Tolerance2 = Tolerance * Tolerance;

        #endregion

        #region Props

        protected override bool WantBackpropagation
        {
            get { return true; }
        }

        #endregion

        #region State

        int count;

        double[] p, r;

        Action nextStep;

        double[,] gradients;

        double[] lastWeights;

        double wError;

        double stepError;

        WeightDecay decay;

        #endregion

        #region Init

        protected internal override void InitializeNewRun(AlgoInitializationMode mode)
        {
            base.InitializeNewRun(mode);

            if (mode == AlgoInitializationMode.Startup)
            {
                count = LearningConnections.ItemArray.Length;
                p = new double[count];
                r = new double[count];
                gradients = new double[2, count];
                lastWeights = new double[count];
                wError = stepError = 1.0;
                decay = ((SCGRule)LearningConnections.ItemArray[0].Rule).WeightDecay;
            }

            // SCG Step 1:

            nextStep = null;
        }

        #endregion

        #region Steps

        protected internal override void BackwardIteration(bool batch, double mse)
        {
            base.BackwardIteration(batch, mse);
            
            if (batch)
            {
                Begin();
            }
        }

        private void Begin()
        {
            WriteLineToDebug("\n-> Next batch iteration. <-");

            if (nextStep == null)
            {
                SCG();
            }
            else
            {
                nextStep();
            }
        }

        private void SCG()
        {
            // Bookmarks:

            Action reset = null, step2 = null, step3 = null, step7 = null;
            
            // -- SCG Step 1 - Init:

            WriteLineToDebug("SCG Step 1 - Init");

            double lambda = StartLambda;
            double lambdaBar = 0;
            bool success = true;
            int k = 1;
            double delta = 0.0;
            double mu = 0.0;
            double errorDelta = 0.0;
            double pProduct = 0.0;
            int resetCounter = 0;

            // p, r.
            InitPR();

            reset = () =>
            {
                WriteLineToDebug("RESET");
                
                lambda = StartLambda;
                lambdaBar = 0;
                success = true;
                k = 1;
                delta = 0.0;
                mu = 0.0;
                errorDelta = 0.0;
                pProduct = 0.0;
                resetCounter = 0;

                // p, r.
                InitPR();

                step2();
            };


            step2 = () =>
            {
                pProduct = GetVectorProduct(p, p);
                
                // -- SCG Step 2 - Second Order Info:

                WriteLineToDebug("SCG Step 2 - Second Order Info");

                if (success)
                {
                    WriteLineToDebug("SCG Step 2 - Success ...");

                    if (pProduct < Tolerance2)
                    {
                        reset();
                    }
                    else
                    {
                        SaveState();

                        double sigma = StartSigma / Math.Sqrt(pProduct);

                        for (int idx = 0; idx < count; idx++)
                        {
                            var conn = ((IBackwardConnection)LearningConnections.ItemArray[idx].Connection);
                            conn.Weight += sigma * p[idx];
                            //if (decay != null)
                            //{
                            //    conn.Weight = decay.GetDecayed(conn.Weight);
                            //}
                        }

                        GetGradients(() =>
                        {
                            delta = 0.0;
                            for (int idx = 0; idx < count; idx++)
                            {
                                double step = (gradients[1, idx] - gradients[0, idx]) / sigma;
                                delta += step * p[idx];
                            }
                            /*GOTO*/
                            step3();
                        });
                    }
                    return;
                }

                /*GOTO*/
                step3();
            };

            step3 = () =>
            {
                // -- SCG Step 3 - Scale:

                WriteLineToDebug("SCG Step 3 - Scale");

                delta += (lambda - lambdaBar) * pProduct;

                // -- SCG Step 4 - Hessian

                WriteLineToDebug("SCG Step 4 - Hessian");

                if (delta <= 0)
                {
                    lambdaBar = 2 * (lambda - delta / pProduct);
                    delta = lambda * pProduct - delta;
                    lambda = lambdaBar;
                }

                // -- SCG Step 5 - Step Size

                WriteLineToDebug("SCG Step 5 - Step Size");

                mu = GetVectorProduct(p, r);
                double alpha = mu / delta;

                // -- SCG Step 6 - Global Delta

                WriteLineToDebug("SCG Step 6 - Error Delta");

                for (int idx = 0; idx < count; idx++)
                {
                    var conn = ((IBackwardConnection)LearningConnections.ItemArray[idx].Connection);
                    conn.Weight = decay.Decayed(lastWeights[idx] + alpha * p[idx]); // Recalculate from w(k)!
                }

                GetStepError(() =>
                {
                    errorDelta = (2.0 * delta * (wError - stepError)) / (mu * mu);

                    /*GOTO*/
                    step7();
                });
            };

            step7 = () =>
            {
                // -- SCG Step 7 - Weight Update:

                WriteLineToDebug("SCG Step 7 - Weight Update. Error delta: " + errorDelta);

                bool restart = false;
                
                if (errorDelta >= 0)
                {
                    // Weights already on alpha*p(k)!

                    double rSum = 0.0;
                    for (int idx = 0; idx < count; idx++)
                    {
                        double tmp = -gradients[1, idx];
                        rSum += tmp * r[idx];
                        r[idx] = tmp;
                    }
                    lambdaBar = 0;
                    success = true;

                    // -- SCG Step 7a.

                    WriteLineToDebug("SCG Step 7a. k = " + k);

                    if (k >= count)
                    {
                        // Restart:

                        WriteLineToDebug("SCG Step 7a. - Restart");

                        restart = true;
                    }
                    else
                    {
                        WriteLineToDebug("SCG Step 7a. - New Direction");
                        
                        // Compute new conjugate direction.
                        double beta = (GetVectorProduct(r, r) - rSum) / mu;

                        // Update direction vector.
                        for (int idx = 0; idx < count; idx++)
                        { 
                            p[idx] = r[idx] + beta * p[idx];
                        }                            
                    }

                    // -- SCG Step 7b.

                    WriteLineToDebug("SCG Step 7b.");

                    if (errorDelta >= 0.75)
                    {
                        //lambda *= 0.5;
                        lambda *= 0.25;
                    }
                }
                else
                {
                    // Error reduction not possible.

                    // Go back to w(k):
                    RestoreState();

                    lambdaBar = lambda;
                    success = false;
                }

                // -- SCG Step 8 - Inc. Scale:

                WriteLineToDebug("SCG Step 8 - Inc. Scale");

                if (errorDelta < 0.25)
                {
                    //lambda *= 4.0;
                    lambda += delta * (1 - errorDelta) / pProduct;
                }

                if (lambda > MaxLambda)
                {
                    lambda = MaxLambda;
                }

                // -- SCG Step 9 - End SCG

                WriteLineToDebug("SCG Step 9 - End SCG");

                if (restart)
                {
                    lambda = StartLambda;
                    lambdaBar = 0;
                    k = 1;
                    resetCounter = 0;
                    
                    r.CopyTo(p, 0);
                }
                else
                {
                    k++;
                }

                if (!success && !restart)
                {
                    if (resetCounter != 0)
                    {
                        reset();
                        return;
                    }
                    resetCounter++;
                }
                else
                {
                    resetCounter = 0;
                }

                /*GOTO*/
                step2();
            };

            step2(); // Initialized -> GOTO step2
        }

        private void InitPR()
        {
            for (int idx = 0; idx < count; idx++)
            {
                var conn = ((IBackwardConnection)LearningConnections.ItemArray[idx].Connection);
                p[idx] = r[idx] = -GetGradient(conn);
            }
        }

        private void SaveState()
        {
            WriteLineToDebug("Saving Weights, Gradients and wError: " + CurrentMSE);

            for (int idx = 0; idx < count; idx++)
            {
                var conn = ((IBackwardConnection)LearningConnections.ItemArray[idx].Connection);
                lastWeights[idx] = conn.Weight;
                gradients[0, idx] = GetGradient(conn);
            }

            wError = CurrentMSE;
        }

        private void RestoreState()
        {
            WriteLineToDebug("Restoring Weights ... ");

            for (int idx = 0; idx < count; idx++)
            {
                var conn = ((IBackwardConnection)LearningConnections.ItemArray[idx].Connection);
                conn.Weight = lastWeights[idx];
            }
        }

        private void GetGradients(Action after)
        {
            WriteToDebug("Getting gradients ... ");
            
            // After Next Training Iteration:
            nextStep = () =>
            {
                for (int idx = 0; idx < count; idx++)
                {
                    var conn = ((IBackwardConnection)LearningConnections.ItemArray[idx].Connection);
                    gradients[1, idx] = GetGradient(conn);
                }

                WriteLineToDebug("Done.");

                after();
            };
        }

        private void GetStepError(Action after)
        {
            WriteToDebug("Getting step error ... ");

            nextStep = () =>
            {
                for (int idx = 0; idx < count; idx++)
                {
                    var conn = ((IBackwardConnection)LearningConnections.ItemArray[idx].Connection);
                    gradients[1, idx] = GetGradient(conn);
                }
                stepError = CurrentMSE;

                WriteLineToDebug("Done. Error: " + stepError);

                after();
            };
        }

        private static double GetGradient(IBackwardConnection conn)
        {
            return -conn.BackwardValues.AvgGradient;
        }

        static double GetVectorProduct(double[] a, double[] b)
        {
            int length = a.Length;
            double value = 0;

            for (int i = 0; i < length; ++i)
                value += a[i] * b[i];

            return value;
        }

        #endregion

        #region DEBUG

        private static bool IsDebugInfoEnabled()
        {
            return false;
        }

        [Conditional("DEBUG")]
        private static void WriteToDebug(string str)
        {
            if (IsDebugInfoEnabled()) Debug.Write(str);
        }

        [Conditional("DEBUG")]
        private static void WriteLineToDebug(string str)
        {
            if (IsDebugInfoEnabled()) Debug.WriteLine(str);
        }

        #endregion
    }
}
