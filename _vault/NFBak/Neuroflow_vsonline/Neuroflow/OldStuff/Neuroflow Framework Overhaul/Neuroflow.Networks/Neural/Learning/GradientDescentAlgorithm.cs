using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core.ComputationAPI;
using System.Threading.Tasks;

namespace Neuroflow.Networks.Neural.Learning
{
    public class GradientDescentAlgorithm : GradientDescentAlgorithm<GradientDescentRule> { }
    
    public class GradientDescentAlgorithm<T> : BackwardLearningAlgorithm<T>
        where T : GradientDescentRule
    {
        #region Props and Fields

        double[] deltaVector;

        public override BackwardIterationMode BackwardIterationMode
        {
            get { return BackwardIterationMode.EnabledAndBackpropagate; }
        } 

        #endregion

        #region Init

        protected override unsafe void InitializeNewRun(double* values, InputValueIndexes* inputValueIndexes, OutputValueIndexes* outputValueIndexes)
        {
            if (deltaVector == null)
            {
                deltaVector = new double[ValueCount];
            }
            else
            {
                fixed (double* d = this.deltaVector)
                {
                    Rtl.ZeroMemory((IntPtr)d, sizeof(double) * ValueCount);
                }
            }
        }

        #endregion

        #region Learn

        protected override unsafe void BatchBackwardIteration(double* values, InputValueIndexes* inputValueIndexes, OutputValueIndexes* outputValueIndexes)
        {
            double momentum = Rule.Momentum;
            double stepSize = Rule.StepSize;
            fixed (double* deltaVector = this.deltaVector)
            {
                for (int i = 0; i < ValueCount; i++)
                {
                    double update = (momentum * deltaVector[i]) + (values[outputValueIndexes[i].GradientSumValueIndex] / LastBatchSize) * stepSize;
                    values[inputValueIndexes[i].WeightValueIndex] += update;
                    deltaVector[i] = update;
                }
            }           
        }

        protected unsafe void DoBatchWeightUpdate(double* values, InputValueIndexes* inputValueIndexes, OutputValueIndexes* outputValueIndexes, double* stepSizes)
        {
            double momentum = Rule.Momentum;
            fixed (double* deltaVector = this.deltaVector)
            {
                for (int i = 0; i < ValueCount; i++)
                {
                    double update = (momentum * deltaVector[i]) + (values[outputValueIndexes[i].GradientSumValueIndex] / LastBatchSize) * stepSizes[i];
                    values[inputValueIndexes[i].WeightValueIndex] += update;
                    deltaVector[i] = update;
                }
            }
        }

        protected override unsafe void StochasticBackwardIteration(double* values, InputValueIndexes* inputValueIndexes, OutputValueIndexes* outputValueIndexes)
        {
            double momentum = Rule.Momentum;
            double stepSize = Rule.StepSize;
            fixed (double* deltaVector = this.deltaVector)
            {
                for (int i = 0; i < ValueCount; i++)
                {
                    double update = (momentum * deltaVector[i]) + values[outputValueIndexes[i].GradientValueIndex] * stepSize;
                    values[inputValueIndexes[i].WeightValueIndex] += update;
                    deltaVector[i] = update;
                }
            }            
        }

        protected unsafe void DoStochasticWeightUpdate(double* values, InputValueIndexes* inputValueIndexes, OutputValueIndexes* outputValueIndexes, double* stepSizes)
        {
            double momentum = Rule.Momentum;
            fixed (double* deltaVector = this.deltaVector)
            {
                for (int i = 0; i < ValueCount; i++)
                {
                    double update = (momentum * deltaVector[i]) + values[outputValueIndexes[i].GradientValueIndex] * stepSizes[i];
                    values[inputValueIndexes[i].WeightValueIndex] += update;
                    deltaVector[i] = update;
                }
            }
        }

        #endregion
    }
}
