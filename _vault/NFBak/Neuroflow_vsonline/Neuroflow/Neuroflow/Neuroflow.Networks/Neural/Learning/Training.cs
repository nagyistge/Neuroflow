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

    public sealed class UnorderedTraining : Training
    {
        internal UnorderedTraining(NeuralNetwork network, BufferAllocator allocator) :
            base(network, allocator, TrainingMode.Unordered)
        {
            Contract.Requires(network != null);
            Contract.Requires(allocator != null);
        }
    }

    public sealed class StreamedTraining : Training
    {
        internal StreamedTraining(NeuralNetwork network, BufferAllocator allocator) :
            base(network, allocator, TrainingMode.Streamed)
        {
            Contract.Requires(network != null);
            Contract.Requires(allocator != null);
        }
    }

    [Serializable]
    public abstract class Training : NeuralSupervisedBatchExecution
    {
        #region Construct

        internal Training(NeuralNetwork network, BufferAllocator allocator, TrainingMode mode)
            : base(network, DetermineIterationRepeat(network), allocator)
        {
            Contract.Requires(network != null);
            Contract.Requires(allocator != null);

            Mode = mode;

            if (Mode == TrainingMode.Streamed && (GCAlgo != GradientComputingAlgorithm.RTLR || GCAlgo != GradientComputingAlgorithm.None))
            {
                throw new InvalidOperationException("Only RTLR allowed for Streamed training. You have to use Recurrent NN with RTLR Algorithm in RecurrentOptions.");
            }

            if ((network.StructuralElementFlags & NNStructuralElement.GradientInformation) != 0)
            {
                if (network.IsRecurrent)
                {
                    GCAlgo = network.RecurrentOptions.Algorithm == RLAlgorithm.BPTT ? GradientComputingAlgorithm.BPTT : GradientComputingAlgorithm.RTLR;
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

            if (GCAlgo == GradientComputingAlgorithm.BPTT)
            {
                savedErrorVectors = new ErrorVectorStack(network, allocator);
            }
        }

        private static int DetermineIterationRepeat(NeuralNetwork Network)
        {
            return 5; // TODO: Fix this

            int max = 1;
            foreach (var bwRule in Network.Layers.OfType<IHasLearningRules>().SelectMany(l => l.LearningRules).OfType<ErrorBasedLearningRule>())
            {
                if (bwRule.IterationRepeat > max) max = bwRule.IterationRepeat;
            }

            return max;
        }

        #endregion

        #region Props and Fields

        ErrorVectorStack savedErrorVectors;

        internal TrainingMode Mode { get; private set; }

        internal GradientComputingAlgorithm GCAlgo { get; private set; }


        #endregion

        #region Execute Logic

        #region Init

        private void EnsureInitialized(NeuralComputationContext context)
        {
            if (!context.Training_IsInitialized)
            {
                // Call ago init first:
                Network.Reset(context, NeuralNetworkResetTarget.Algorithms);

                if (savedErrorVectors != null) savedErrorVectors.Initialize(context);

                context.Training_IsInitialized = true;
            }
        }

        #endregion

        #region Overrides

        #region Iteration

        // Batch Iterations (all repeated):
        protected override void DoExecuteBatch(VectorComputationContext context, VectorBuffer<float> vectorBuffer, VectorFlowBatch<float> batch)
        {
            // This is where batch iterations begins

            var ctx = (NeuralComputationContext)context;

            EnsureInitialized(ctx);

            var neuralBatch = batch as NeuralVectorFlowBatch;

            if (neuralBatch != null && Mode == TrainingMode.Streamed && neuralBatch.ResetSchedule == ResetSchedule.BeforeExecution)
            {
                // Streamed reset scheduled:
                ResetAll(ctx);
            }

            base.DoExecuteBatch(context, vectorBuffer, batch);

            if (neuralBatch != null && Mode == TrainingMode.Streamed && neuralBatch.ResetSchedule == ResetSchedule.AfterExecution)
            {
                // Streamed reset scheduled:
                ResetAll(ctx);
            }
        }

        // Batch Iteration:
        protected override void ExecuteBatchIteration(VectorComputationContext context, VectorBuffer<float> vectorBuffer, VectorFlowBatch<float> batch)
        {
            var ctx = (NeuralComputationContext)context;

            // Begin of batch:
            ctx.Training_BeginOfBatch = true;

            if (Mode == TrainingMode.Unordered)
            {
                // Reset backward values, and clear error information:
                ResetGradientComputingValues(ctx);
                ctx.Training_NumberOfIterationsInBatch = 0;
            }

            base.ExecuteBatchIteration(context, vectorBuffer, batch);

            // Batch done, iterate algos:
            Network.CallErrorBasedBatchLearningAlgorithms(ctx, ctx.Training_NumberOfIterationsInBatch, AverageErrorBuffer);
        }

        // A Flow:
        protected override void DoExecuteVectorFlow(VectorComputationContext context, VectorBuffer<float> vectorBuffer, VectorFlow<float> vectorFlow)
        {
            var ctx = (NeuralComputationContext)context;

            if (Mode == TrainingMode.Unordered)
            {
                // Reset forward values:
                ResetNetworkValues(ctx);

                // Reset backward errors and gradients:
                // If beginOfBatch == true, 
                // backward errors and gradients already reseted by a ResetBackwardValues() call
                if (!ctx.Training_BeginOfBatch)
                {
                    ResetPartialGradientComputingValues(ctx);
                }
            }
            
            // Begin of recurrent flow:
            ctx.Training_BPTTEllapsedForwardIterationCount = 0;

            base.DoExecuteVectorFlow(context, vectorBuffer, vectorFlow);

            ctx.Training_BeginOfBatch = false;
        }

        // Each vector iteration:
        protected override void Iteration(VectorComputationContext context)
        {
            var ctx = (NeuralComputationContext)context;

            if (savedErrorVectors != null) // aka BPTT
            {
                Network.CallBeforeIterationLearningAlgorithms(ctx, ctx.Training_BeginOfBatch);
                Network.Iteration(ctx, true, ctx.Training_BPTTEllapsedForwardIterationCount++);
            }
            else
            {
                Network.CallBeforeIterationLearningAlgorithms(ctx, ctx.Training_BeginOfBatch);
                Network.Iteration(ctx, true);
            }
        } 

        #endregion

        #region Errors

        #region Error

        protected override void ComputeError(VectorComputationContext context, BufferedVector<float> desiredOutputVector, int entryIndex, int entryCount)
        {
            var ctx = (NeuralComputationContext)context;

            base.ComputeError(context, desiredOutputVector, entryIndex, entryCount); // ErrorBuffer contains the error vector

            bool callAlgo = true;

            if ((Network.StructuralElementFlags & NNStructuralElement.BackwardImplementation) != 0) // aka BP or BPTT
            {
                callAlgo = BP_ErrorBasedComputed(ctx, entryIndex, entryCount);
            }
            else if (GCAlgo == GradientComputingAlgorithm.RTLR)
            {
                RTLR_ErrorBasedComputed(ctx);
            }

            if (callAlgo) CallErrorBasedStochasticLearningAlgorithms(ctx);
        }

        private void CallErrorBasedStochasticLearningAlgorithms(NeuralComputationContext context)
        {
            Network.CallErrorBasedStochasticLearningAlgorithms(context, LastErrorBuffer);
            context.Training_NumberOfIterationsInBatch++;
        }

        #endregion

        #region No Error

        protected override void NoErrorAtCurrentIteration(VectorComputationContext context)
        {
            var ctx = (NeuralComputationContext)context;

            if (savedErrorVectors != null) // aka BPTT
            {
                // Push null vector:
                savedErrorVectors.PushNull(ctx);
            }
            else if (GCAlgo == GradientComputingAlgorithm.RTLR)
            {
                // Compute P Values:
                Network.PropagatePValues(ctx, null);
            }
        } 

        #endregion

        #region BP

        private bool BP_ErrorBasedComputed(NeuralComputationContext context, int entryIndex, int entryCount)
        {
            Debug.Assert(GCAlgo == GradientComputingAlgorithm.BP || GCAlgo == GradientComputingAlgorithm.BPTT);

            // Errors registered, backpropagate:
            if (savedErrorVectors != null) // aka BPTT
            {
                return BP_ErrorBasedComputed_BPTT(context, entryIndex, entryCount);
            }
            else // aka BP
            {
                BP_ErrorBasedComputed_FF(context);
                return true;
            }
        }

        #region FF

        private void BP_ErrorBasedComputed_FF(NeuralComputationContext context)
        {
            Network.SetError(context, LastErrorBuffer);
            Network.Backpropagate(context, BackprogrationMode.FeedForward);
        }  

        #endregion

        #region BPTT

        private bool BP_ErrorBasedComputed_BPTT(NeuralComputationContext context, int entryIndex, int entryCount)
        {
            savedErrorVectors.Push(context, LastErrorBuffer);

            if (entryIndex == entryCount - 1)
            {
                DoBPTT(context);
                return true;
            }
            return false;
        }

        private void DoBPTT(NeuralComputationContext context)
        {
            Debug.Assert(GCAlgo == GradientComputingAlgorithm.BPTT);
            Debug.Assert(savedErrorVectors != null);
            Debug.Assert(savedErrorVectors.GetSize(context) == context.Training_BPTTEllapsedForwardIterationCount);

            bool lastErrorsSet = false;
            for (int iterationIndex = 0; iterationIndex < context.Training_BPTTEllapsedForwardIterationCount; iterationIndex++)
            {
                int storedValueIndex = (context.Training_BPTTEllapsedForwardIterationCount - 1) - iterationIndex;
                var errors = savedErrorVectors.Pop(context);

                if (errors != null)
                {
                    Network.SetError(context, errors.Value);
                    lastErrorsSet = true;
                }
                else if (lastErrorsSet)
                {
                    ZeroErrorInterface(context);
                    lastErrorsSet = false;
                }

                Network.Backpropagate(context, (iterationIndex == context.Training_BPTTEllapsedForwardIterationCount - 1) ? BackprogrationMode.BPTTLastStep : BackprogrationMode.BPTTInternalStep, storedValueIndex);
            }
        }

        private void ZeroErrorInterface(NeuralComputationContext context)
        {
            Network.ZeroBuffer(context, LastErrorBuffer);
            Network.SetError(context, LastErrorBuffer);
        } 

        #endregion

        #endregion

        #region RTLR

        private void RTLR_ErrorBasedComputed(NeuralComputationContext context)
        {
            Network.PropagatePValues(context, LastErrorBuffer);
        } 

        #endregion

        #endregion

        #endregion

        #region Helpers

        private void ResetPartialGradientComputingValues(NeuralComputationContext context)
        {
            if (GCAlgo == GradientComputingAlgorithm.BPTT)
            {
                Network.Reset(context, NeuralNetworkResetTarget.Errors | NeuralNetworkResetTarget.Gradients);
            }
            else if (GCAlgo == GradientComputingAlgorithm.RTLR)
            {
                Network.Reset(context, NeuralNetworkResetTarget.Ps);
            }
        }

        private void ResetNetworkValues(NeuralComputationContext context)
        {
            if (Network.IsRecurrent)
            {
                Network.Reset(context, NeuralNetworkResetTarget.Outputs);
            }
        }

        private void ResetGradientComputingValues(NeuralComputationContext context)
        {
            if (GCAlgo == GradientComputingAlgorithm.BPTT)
            {
                Network.Reset(context, NeuralNetworkResetTarget.Errors | NeuralNetworkResetTarget.Gradients | NeuralNetworkResetTarget.GradientSums);
            }
            else if (GCAlgo == GradientComputingAlgorithm.RTLR)
            {
                Network.Reset(context, NeuralNetworkResetTarget.Ps | NeuralNetworkResetTarget.GradientSums);
            }
            else if (GCAlgo == GradientComputingAlgorithm.BP)
            {
                Network.Reset(context, NeuralNetworkResetTarget.GradientSums);
            }
        }

        private void ResetAll(NeuralComputationContext context)
        {
            Network.Reset(context, NeuralNetworkResetTarget.All);
        }

        #endregion

        #endregion
    }
}
