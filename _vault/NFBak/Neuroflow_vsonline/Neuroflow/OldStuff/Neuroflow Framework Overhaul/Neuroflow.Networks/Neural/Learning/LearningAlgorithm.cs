using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core.ComputationAPI;
using System.Diagnostics.Contracts;
using System.Diagnostics;

namespace Neuroflow.Networks.Neural.Learning
{
    public enum BackwardIterationMode { Disabled, Enabled, EnabledAndBackpropagate }

    public struct InputValueIndexes
    {
        public int InputValueIndex;

        public int WeightValueIndex;
    }

    public struct OutputValueIndexes
    {
        public int GradientValueIndex;

        public int GradientSumValueIndex;
    }
    
    public abstract class LearningAlgorithm
    {
        #region Props and Fields

        protected double LastAverageError { get; private set; }

        protected double LastBatchSize { get; private set; }

        protected internal LearningRule Rule { get; private set; }

        internal ValueSpace<double> ValueSpace { get; private set; }

        InputValueIndexes[] inputValueIndexes;

        OutputValueIndexes[] outputValueIndexes;

        protected int ValueCount { get; private set; }

        public abstract bool WantForwardIteration { get; }

        public abstract BackwardIterationMode BackwardIterationMode { get; }

        #endregion

        #region Init

        internal void Initialize(
            LearningRule rule, 
            ValueSpace<double> valueSpace,
            InputValueIndexes[] inputValueIndexes,
            OutputValueIndexes[] outputValueIndexes)
        {
            Contract.Requires(rule != null);
            Contract.Requires(valueSpace != null);
            Contract.Requires(inputValueIndexes != null);
            Contract.Requires(outputValueIndexes != null);
            Contract.Requires(inputValueIndexes.Length == outputValueIndexes.Length);

            Rule = rule;
            ValueSpace = valueSpace;
            this.inputValueIndexes = inputValueIndexes;
            this.outputValueIndexes = outputValueIndexes;
            ValueCount = inputValueIndexes.Length;

            InitializeNewRun(true);
        }

        #endregion

        #region Learning Protocol

        #region Init

        unsafe internal void InitializeNewRun(bool isTrainingInitialization = false)
        {
            LastAverageError = double.MaxValue;

            double* values = (double*)(ValueSpace.Ptr);
            fixed (InputValueIndexes* inputValueIndexes = this.inputValueIndexes)
            fixed (OutputValueIndexes* outputValueIndexes = this.outputValueIndexes)
            {
                if (isTrainingInitialization) InitializeNewTraining(values, inputValueIndexes, outputValueIndexes);
                InitializeNewRun(values, inputValueIndexes, outputValueIndexes);
            }
        }

        unsafe protected virtual void InitializeNewTraining(double* values, InputValueIndexes* inputValueIndexes, OutputValueIndexes* outputValueIndexes)
        {
        }

        unsafe protected virtual void InitializeNewRun(double* values, InputValueIndexes* inputValueIndexes, OutputValueIndexes* outputValueIndexes)
        {
        } 

        #endregion

        #region Forward

        unsafe internal void ForwardIteration(bool isNewBatch)
        {
            double* values = (double*)(ValueSpace.Ptr);
            fixed (InputValueIndexes* inputValueIndexes = this.inputValueIndexes)
            {
                ForwardIteration(isNewBatch, values, inputValueIndexes);
            }
        }

        unsafe protected virtual void ForwardIteration(bool isNewBatch, double* values, InputValueIndexes* inputValueIndexes)
        {
        } 

        #endregion

        #region Backward

        unsafe internal void BackwardIteration(bool isBatchIteration, int batchSize, double averageError)
        {
            LastAverageError = averageError;
            LastBatchSize = batchSize;
            double* values = (double*)ValueSpace.Ptr;
            fixed (InputValueIndexes* inputValueIndexes = this.inputValueIndexes)
            fixed (OutputValueIndexes* outputValueIndexes = this.outputValueIndexes)
            {
                BackwardIteration(isBatchIteration, values, inputValueIndexes, outputValueIndexes);
            }
        }

        unsafe protected virtual void BackwardIteration(bool isBatchIteration, double* values, InputValueIndexes* inputValueIndexes, OutputValueIndexes* outputValueIndexes)
        {
        } 

        #endregion

        #endregion
    }
}
