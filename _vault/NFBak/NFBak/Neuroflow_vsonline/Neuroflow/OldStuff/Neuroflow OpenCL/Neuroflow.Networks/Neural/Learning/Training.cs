using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Neuroflow.Core.Vectors;
using System.Diagnostics;

namespace Neuroflow.Networks.Neural.Learning
{
    internal enum TrainingMode : byte { Unordered, Streamed }

    internal enum GradientComputingAlgorithm : byte { None, BP, BPTT, RTLR }

    [Serializable]
    public abstract class Training : NeuralVectorFlowBatchExecution
    {
        #region Construct

        internal Training(NeuralNetwork Network, TrainingMode mode)
            : base(Network, DetermineIterationRepeatPars(Network))
        {
            Contract.Requires(Network != null);

            Mode = mode;

            if (Mode == TrainingMode.Streamed && (GCAlgo != GradientComputingAlgorithm.RTLR || GCAlgo != GradientComputingAlgorithm.None))
            {
                throw new InvalidOperationException("Only RTLR allowed for Streamed training. You have to use Recurrent NN with RTLR Algorithm in RecurrentOptions.");
            }

            if ((Network.StructuralElementFlags & NNStructuralElement.GradientInformation) != 0)
            {
                if (Network.IsRecurrent)
                {
                    GCAlgo = Network.RecurrentOptions.Algorithm == RLAlgorithm.BPTT ? GradientComputingAlgorithm.BPTT : GradientComputingAlgorithm.RTLR;
                }
                else
                {
                    GCAlgo = GradientComputingAlgorithm.BP;
                }
            }
            else
            {
                GCAlgo = GradientComputingAlgorithm.None;
            }
        }

        private static IterationRepeatPars DetermineIterationRepeatPars(NeuralNetwork Network)
        {
            int min = int.MaxValue;
            int max = int.MinValue;
            foreach (var bwRule in Network.Layers.OfType<IHasLearningRules>().SelectMany(l => l.LearningRules).OfType<ErrorBasedLearningRule>())
            {
                if (bwRule.IterationRepeat != null)
                {
                    if (bwRule.IterationRepeat.MinIterations < min) min = bwRule.IterationRepeat.MinIterations;
                    if (bwRule.IterationRepeat.MaxIterations > max) max = bwRule.IterationRepeat.MaxIterations;
                }
            }

            if (min != int.MaxValue)
            {
                Debug.Assert(max != int.MinValue);
                return new IterationRepeatPars(min, max);
            }

            return null;
        }

        #endregion

        #region Props and Fields

        internal TrainingMode Mode { get; private set; }

        internal GradientComputingAlgorithm GCAlgo { get; private set; }

        #endregion

        #region Execute Logic

        #region State

        bool isInitialized;

        bool beginOfBatch;

        int bpttEllapsedForwardIterationCount;

        int numberOfIterationsInBatch;

        ErrorVectorStack savedErrorVectors; 

        #endregion

        #region Create Validation

        public Validation CreateValidation()
        {
            return new Validation(Network);
        }

        #endregion

        #region Init

        private void EnsureInitialized()
        {
            if (!isInitialized)
            {
                // Call ago init first:
                Network.Reset(NeuralNetworkResetTarget.Algorithms);
                if (GCAlgo == GradientComputingAlgorithm.BPTT)
                {
                    savedErrorVectors = new ErrorVectorStack(Network.RecurrentOptions.MaxIterations, Network.OutputInterfaceLength);
                }
                isInitialized = true;
            }
        }

        #endregion

        #region Overrides

        #region Iteration

        // Batch Iterations:
        protected override BatchExecutionResult LockedDoBatchExcuteIterations(VectorFlowBatch<float> batch, Action<BatchExecutionResult> iterationCallback)
        {
            // This is where batch iterations begins

            EnsureInitialized();

            var neuralBatch = batch as NeuralVectorFlowBatch;

            if (neuralBatch != null && Mode == TrainingMode.Streamed && neuralBatch.ResetSchedule == ResetSchedule.BeforeExecution)
            {
                // Streamed reset scheduled:
                ResetAll();
            }

            var result = base.LockedDoBatchExcuteIterations(batch, iterationCallback);

            if (neuralBatch != null && Mode == TrainingMode.Streamed && neuralBatch.ResetSchedule == ResetSchedule.AfterExecution)
            {
                // Streamed reset scheduled:
                ResetAll();
            }

            return result;
        }

        // Batch Iteration:
        protected override double ExecuteBatchIteration(VectorFlowBatch<float> batch, double[] resultErrors)
        {
            // Begin of batch:
            beginOfBatch = true;

            if (Mode == TrainingMode.Unordered)
            {
                // Reset backward values, and clear error information:
                ResetGradientComputingValues();
                numberOfIterationsInBatch = 0;
            }

            double error = base.ExecuteBatchIteration(batch, resultErrors);

            Debug.Assert(batch.VectorFlows.Count == resultErrors.Length);
            Debug.Assert(numberOfIterationsInBatch >= resultErrors.Length);

            // Batch done, iterate algos:
            Network.CallErrorBasedBatchLearningAlgorithms(numberOfIterationsInBatch, error);

            // Batch done. Yeah.
            return error;
        }

        // A Flow:
        protected override double LockedExecuteVectorFlow(VectorFlow<float> vectorFlow)
        {
            if (Mode == TrainingMode.Unordered)
            {
                // Reset forward values:
                ResetNetworkValues();

                // Reset backward errors and gradients:
                // If beginOfBatch == true, 
                // backward errors and gradients already reseted by a ResetBackwardValues() call
                if (!beginOfBatch)
                {
                    ResetPartialGradientComputingValues();
                }
            }
            
            // Begin of recurrent flow:
            bpttEllapsedForwardIterationCount = 0;

            var result = base.LockedExecuteVectorFlow(vectorFlow);

            beginOfBatch = false;

            return result;
        }

        // Each vector iteration:
        protected override void Iteration()
        {
            if (savedErrorVectors != null) // aka BPTT
            {
                Network.CallBeforeIterationLearningAlgorithms(beginOfBatch);
                Network.Iteration(true, bpttEllapsedForwardIterationCount++);
            }
            else
            {
                Network.CallBeforeIterationLearningAlgorithms(beginOfBatch);
                Network.Iteration(true);
            }
        } 

        #endregion

        #region Errors

        #region Error

        protected override double? ComputeError(float?[] desiredOutputVector, int entryIndex, int entryCount)
        {
            double? error = base.ComputeError(desiredOutputVector, entryIndex, entryCount); // ErrorBuffer contains the error vector

            if (error == null) return null;

            bool callAlgo = true;

            if ((Network.StructuralElementFlags & NNStructuralElement.BackwardImplementation) != 0) // aka BP or BPTT
            {
                callAlgo = BP_ErrorBasedComputed(entryIndex, entryCount);
            }
            else if (GCAlgo == GradientComputingAlgorithm.RTLR)
            {
                RTLR_ErrorBasedComputed();
            }

            if (callAlgo) CallErrorBasedStochasticLearningAlgorithms(error.Value);

            return error;
        }

        private void CallErrorBasedStochasticLearningAlgorithms(double error)
        {
            Network.CallErrorBasedStochasticLearningAlgorithms(error);
            numberOfIterationsInBatch++;
        }

        #endregion

        #region No Error

        protected override void NoErrorAtCurrentIteration()
        {
            if (savedErrorVectors != null) // aka BPTT
            {
                // Push null vector:
                savedErrorVectors.PushNull();
            }
            else if (GCAlgo == GradientComputingAlgorithm.RTLR)
            {
                // Compute P Values:
                Network.PropagatePValues(null);
            }
        } 

        #endregion

        #region BP

        private bool BP_ErrorBasedComputed(int entryIndex, int entryCount)
        {
            Debug.Assert(GCAlgo == GradientComputingAlgorithm.BP || GCAlgo == GradientComputingAlgorithm.BPTT);

            // Errors registered, backpropagate:
            if (savedErrorVectors != null) // aka BPTT
            {
                return BP_ErrorBasedComputed_BPTT(entryIndex, entryCount);
            }
            else // aka BP
            {
                BP_ErrorBasedComputed_FF();
                return true;
            }
        }

        #region FF

        private void BP_ErrorBasedComputed_FF()
        {
            Network.WriteError(ErrorBuffer);
            Network.Backpropagate(BackprogrationMode.FeedForward);
        }  

        #endregion

        #region BPTT

        private bool BP_ErrorBasedComputed_BPTT(int entryIndex, int entryCount)
        {
            savedErrorVectors.Push(ErrorBuffer);

            if (entryIndex == entryCount - 1)
            {
                DoBPTT();
                return true;
            }
            return false;
        }

        private void DoBPTT()
        {
            Debug.Assert(GCAlgo == GradientComputingAlgorithm.BPTT);
            Debug.Assert(savedErrorVectors != null);
            Debug.Assert(savedErrorVectors.Size == bpttEllapsedForwardIterationCount);

            bool lastErrorsSet = false;
            for (int iterationIndex = 0; iterationIndex < bpttEllapsedForwardIterationCount; iterationIndex++)
            {
                int storedValueIndex = (bpttEllapsedForwardIterationCount - 1) - iterationIndex;
                float[] errors = savedErrorVectors.Pop();

                if (errors != null)
                {
                    Network.WriteError(errors);
                    lastErrorsSet = true;
                }
                else if (lastErrorsSet)
                {
                    ZeroErrorInterface();
                    lastErrorsSet = false;
                }

                Network.Backpropagate((iterationIndex == bpttEllapsedForwardIterationCount - 1) ? BackprogrationMode.BPTTLastStep : BackprogrationMode.BPTTInternalStep, storedValueIndex);
            }
        } 

        #endregion

        #endregion

        #region RTLR

        private void RTLR_ErrorBasedComputed()
        {
            Network.PropagatePValues(ErrorBuffer);
        } 

        #endregion

        #endregion

        #endregion

        #region Helpers

        private void ZeroErrorInterface()
        {
            Debug.Assert((Network.StructuralElementFlags & NNStructuralElement.BackwardImplementation) != 0);

            Array.Clear(ErrorBuffer, 0, ErrorBuffer.Length);
            Network.WriteError(ErrorBuffer);
        }

        private void ResetPartialGradientComputingValues()
        {
            if (GCAlgo == GradientComputingAlgorithm.BPTT)
            {
                Network.Reset(NeuralNetworkResetTarget.Errors | NeuralNetworkResetTarget.Gradients);
            }
            else if (GCAlgo == GradientComputingAlgorithm.RTLR)
            {
                Network.Reset(NeuralNetworkResetTarget.Ps);
            }
        }

        private void ResetNetworkValues()
        {
            if (Network.IsRecurrent)
            {
                Network.Reset(NeuralNetworkResetTarget.Outputs);
            }
        }

        private void ResetGradientComputingValues()
        {
            if (GCAlgo == GradientComputingAlgorithm.BPTT)
            {
                Network.Reset(NeuralNetworkResetTarget.Errors | NeuralNetworkResetTarget.Gradients | NeuralNetworkResetTarget.GradientSums);
            }
            else if (GCAlgo == GradientComputingAlgorithm.RTLR)
            {
                Network.Reset(NeuralNetworkResetTarget.Ps | NeuralNetworkResetTarget.GradientSums);
            }
            else if (GCAlgo == GradientComputingAlgorithm.BP)
            {
                Network.Reset(NeuralNetworkResetTarget.GradientSums);
            }
        }

        private void ResetAll()
        {
            Network.Reset(NeuralNetworkResetTarget.All);
        }

        #endregion

        #endregion
    }
}
