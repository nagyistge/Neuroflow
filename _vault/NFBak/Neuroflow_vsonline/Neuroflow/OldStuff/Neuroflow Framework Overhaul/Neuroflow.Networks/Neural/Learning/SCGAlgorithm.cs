using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Neuroflow.Networks.Neural.Learning
{
    //unsafe delegate void SCGStep(double* values, InputValueIndexes* inputValueIndexes, OutputValueIndexes* outputValueIndexes);
    
    unsafe public sealed class SCGAlgorithm : BackwardLearningAlgorithm<SCGRule>
    {
        #region Constants

        const double StartSigma = 0.0001;

        const double StartLambda = 0.000001;

        const double MaxLambda = double.MaxValue / 100;

        const double Tolerance = 0.00000001;

        const double Tolerance2 = Tolerance * Tolerance;

        #endregion

        #region Props and Fields

        public override BackwardIterationMode BackwardIterationMode
        {
            get { return BackwardIterationMode.EnabledAndBackpropagate; }
        }

        Action nextStep;

        double* values;

        InputValueIndexes* inputValueIndexes;

        OutputValueIndexes* outputValueIndexes;

        double[] pArray, rArray, gradients0Array, gradients1Array, lastWeightsArray;

        double* p, r, gradients0, gradients1, lastWeights;

        double wError;

        double stepError;

        double lambda;

        double lambdaBar;

        bool success;

        int k;

        double delta;

        double mu;

        double errorDelta;

        double pProduct;

        int resetCounter;

        #endregion

        #region Init

        protected override unsafe void InitializeNewRun(double* values, InputValueIndexes* inputValueIndexes, OutputValueIndexes* outputValueIndexes)
        {
            pArray = new double[ValueCount];
            rArray = new double[ValueCount];
            gradients0Array = new double[ValueCount];
            gradients1Array = new double[ValueCount];
            lastWeightsArray = new double[ValueCount];
            wError = stepError = 1.0;

            // SCG Step 1:

            nextStep = null;
        }

        #endregion

        #region Steps

        protected override unsafe void BatchBackwardIteration(double* values, InputValueIndexes* inputValueIndexes, OutputValueIndexes* outputValueIndexes)
        {
            this.values = values;
            this.inputValueIndexes = inputValueIndexes;
            this.outputValueIndexes = outputValueIndexes;

            fixed (double* pPtr = pArray, rPtr = rArray, gradients0Ptr = gradients0Array, gradients1Ptr = gradients1Array, lastWeightsPtr = lastWeightsArray)
            {
                p = pPtr;
                r = rPtr;
                gradients0 = gradients0Ptr;
                gradients1 = gradients1Ptr;
                lastWeights = lastWeightsPtr;

                if (nextStep == null)
                {
                    BeginSCG();
                }
                else
                {
                    nextStep();
                }
            }
        }

        unsafe private double GetWeight(int index)
        {
            return values[inputValueIndexes[index].WeightValueIndex];
        }

        unsafe private void SetWeight(int index, double value)
        {
            values[inputValueIndexes[index].WeightValueIndex] = value;
        }

        unsafe private double GetGradient(int index)
        {
            return -(values[outputValueIndexes[index].GradientSumValueIndex] / LastBatchSize);
        }

        Action reset = null, step2 = null, step3 = null, step7 = null;

        private void BeginSCG()
        {
            // Bookmarks:

            WriteLineToDebug("SCG Step 1 - Init");

            // p, r.
            InitPR();

            // Values
            InitValues();

            reset = () =>
            {
                WriteLineToDebug("RESET");

                InitValues();

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

                        for (int idx = 0; idx < ValueCount; idx++)
                        {
                            SetWeight(idx, GetWeight(idx) + sigma * p[idx]);
                        }

                        GetGradients(() =>
                        {
                            delta = 0.0;
                            for (int idx = 0; idx < ValueCount; idx++)
                            {
                                double step = (gradients1[idx] - gradients0[idx]) / sigma;
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

                for (int idx = 0; idx < ValueCount; idx++)
                {
                    SetWeight(idx, lastWeights[idx] + alpha * p[idx]); // Recalculate from w(k)!
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
                    for (int idx = 0; idx < ValueCount; idx++)
                    {
                        double tmp = -gradients1[idx];
                        rSum += tmp * r[idx];
                        r[idx] = tmp;
                    }
                    lambdaBar = 0;
                    success = true;

                    // -- SCG Step 7a.

                    WriteLineToDebug("SCG Step 7a. k = " + k);

                    if (k >= ValueCount)
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
                        for (int idx = 0; idx < ValueCount; idx++)
                        {
                            p[idx] = r[idx] + beta * p[idx];
                        }
                    }

                    // -- SCG Step 7b.

                    WriteLineToDebug("SCG Step 7b.");

                    if (errorDelta >= 0.75)
                    {
                        if (Rule.ScalingMethod == SCGScalingMethod.Moller)
                        {
                            lambda *= 0.5;
                        }
                        else
                        {
                            lambda *= 0.25;
                        }
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
                    if (Rule.ScalingMethod == SCGScalingMethod.Moller)
                    {
                        lambda *= 4.0;
                    }
                    else
                    {
                        lambda += delta * (1 - errorDelta) / pProduct;
                    }
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

                    CopyTo(r, p);
                }
                else
                {
                    k++;
                }

                if (!success && !restart)
                {
                    if (resetCounter >= Rule.FaultTolerance)
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

        private void InitValues()
        {
            lambda = StartLambda;
            lambdaBar = 0;
            success = true;
            k = 1;
            delta = 0.0;
            mu = 0.0;
            errorDelta = 0.0;
            pProduct = 0.0;
            resetCounter = 0;
        }

        private void InitPR()
        {
            for (int idx = 0; idx < ValueCount; idx++)
            {
                p[idx] = r[idx] = -GetGradient(idx);
            }
        }

        private void SaveState()
        {
            WriteLineToDebug("Saving Weights, Gradients and wError: " + LastAverageError);

            for (int idx = 0; idx < ValueCount; idx++)
            {
                lastWeights[idx] = GetWeight(idx);
                gradients0[idx] = GetGradient(idx);
            }

            wError = LastAverageError;
        }

        private void RestoreState()
        {
            WriteLineToDebug("Restoring Weights ... ");

            for (int idx = 0; idx < ValueCount; idx++)
            {
                SetWeight(idx, lastWeights[idx]);
            }
        }

        private void GetGradients(Action after)
        {
            WriteToDebug("Getting gradients ... ");

            // After Next Training Iteration:
            nextStep = () =>
            {
                for (int idx = 0; idx < ValueCount; idx++)
                {
                    gradients1[idx] = GetGradient(idx);
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
                for (int idx = 0; idx < ValueCount; idx++)
                {
                    gradients1[idx] = GetGradient(idx);
                }
                stepError = LastAverageError;

                WriteLineToDebug("Done. Error: " + stepError);

                after();
            };
        }

        unsafe double GetVectorProduct(double* a, double* b)
        {
            double value = 0;

            for (int i = 0; i < ValueCount; ++i) value += a[i] * b[i];

            return value;
        }

        unsafe void CopyTo(double* source, double* target)
        {
            for (int i = 0; i < ValueCount; ++i) target[i] = source[i];
        }

        #endregion

        #region DEBUG

        private static bool IsDebugInfoEnabled()
        {
            return true;
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
