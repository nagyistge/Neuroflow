using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core.ComputationAPI;

namespace Neuroflow.Networks.Neural.Learning
{
    public abstract class AutoAdjustedGradientDescentAlgorithm<T> : GradientDescentAlgorithm<T>
        where T : AutoAdjustedGradientDescentRule
    {
        double[] stepSizesArray;

        double[] lastGradiensArray;

        bool begin;

        protected override unsafe void InitializeNewRun(double* values, InputValueIndexes* inputValueIndexes, OutputValueIndexes* outputValueIndexes)
        {
            base.InitializeNewRun(values, inputValueIndexes, outputValueIndexes);

            if (stepSizesArray == null)
            {
                stepSizesArray = new double[ValueCount];
            }
            else
            {
                fixed (double* d = this.stepSizesArray)
                {
                    Rtl.ZeroMemory((IntPtr)d, sizeof(double) * ValueCount);
                }
            }

            if (lastGradiensArray == null)
            {
                lastGradiensArray = new double[ValueCount];
            }
            else
            {
                fixed (double* d = this.lastGradiensArray)
                {
                    Rtl.ZeroMemory((IntPtr)d, sizeof(double) * ValueCount);
                }
            }

            fixed (double* d = this.stepSizesArray)
            {
                for (int idx = 0; idx < ValueCount; idx++) d[idx] = Rule.InitialStepSize;
            }

            begin = true;
        }

        protected override unsafe void StochasticBackwardIteration(double* values, InputValueIndexes* inputValueIndexes, OutputValueIndexes* outputValueIndexes)
        {
            if (Rule.StochasticAdaptiveStateUpdate)
            {
                fixed (double* stepSizes = stepSizesArray, lastGradients = lastGradiensArray)
                {
                    StepAdatpiveState(values, inputValueIndexes, outputValueIndexes, stepSizes, lastGradients, false);
                    DoStochasticWeightUpdate(values, inputValueIndexes, outputValueIndexes, stepSizes);
                }
            }
            else
            {
                fixed (double* stepSizes = stepSizesArray)
                {
                    DoStochasticWeightUpdate(values, inputValueIndexes, outputValueIndexes, stepSizes);
                }
            }
        }

        protected override unsafe void StochasticEndOfBatchBackwardIteration(double* values, InputValueIndexes* inputValueIndexes, OutputValueIndexes* outputValueIndexes)
        {
            if (!Rule.StochasticAdaptiveStateUpdate)
            {
                fixed (double* stepSizes = stepSizesArray, lastGradients = lastGradiensArray)
                {
                    StepAdatpiveState(values, inputValueIndexes, outputValueIndexes, stepSizes, lastGradients, true);
                }
            }
        }

        protected override unsafe void BatchBackwardIteration(double* values, InputValueIndexes* inputValueIndexes, OutputValueIndexes* outputValueIndexes)
        {
            fixed (double* stepSizes = stepSizesArray, lastGradients = lastGradiensArray)
            {
                StepAdatpiveState(values, inputValueIndexes, outputValueIndexes, stepSizes, lastGradients, true);
                DoBatchWeightUpdate(values, inputValueIndexes, outputValueIndexes, stepSizes);
            }
        }

        private unsafe void StepAdatpiveState(double* values, InputValueIndexes* inputValueIndexes, OutputValueIndexes* outputValueIndexes, double* stepSizes, double* lastGradients, bool useAverageGradients)
        {
            if (begin)
            {
                if (useAverageGradients)
                {
                    for (int idx = 0; idx < ValueCount; idx++) lastGradients[idx] = values[outputValueIndexes[idx].GradientSumValueIndex] / LastBatchSize;
                }
                else
                {
                    for (int idx = 0; idx < ValueCount; idx++) lastGradients[idx] = values[outputValueIndexes[idx].GradientValueIndex];
                }
                begin = false;
            }
            else
            {
                if (useAverageGradients)
                {
                    for (int idx = 0; idx < ValueCount; idx++)
                    {
                        double lastGradient = lastGradients[idx];
                        stepSizes[idx] = Rule.StepSizeRange.Cut(CalculateStepSize(stepSizes[idx], lastGradient, lastGradients[idx] = values[outputValueIndexes[idx].GradientSumValueIndex] / LastBatchSize));
                    }
                }
                else
                {
                    for (int idx = 0; idx < ValueCount; idx++)
                    {
                        double lastGradient = lastGradients[idx];
                        stepSizes[idx] = Rule.StepSizeRange.Cut(CalculateStepSize(stepSizes[idx], lastGradient, lastGradients[idx] = values[outputValueIndexes[idx].GradientValueIndex]));
                    }
                }
            }
        }

        protected abstract unsafe double CalculateStepSize(double currentStepSize, double lastGradient, double currentGradient);
    }
}
