using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core.Vectors;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using System.Diagnostics;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Neuroflow.Core.ComponentModel;
using Neuroflow.Core.ComputationAPI;

namespace Neuroflow.Networks.Neural.Learning
{
    internal enum TrainingMode : byte { Unordered, Streamed }

    [Serializable]
    public abstract class Training : NeuralVectorFlowBatchExecution
    {
        #region Construct

        internal Training(NeuralNetwork network, TrainingMode mode)
            : base(network, DetermineIterationRepeatPars(network))
        {
            Contract.Requires(network != null);

            Mode = mode;
        }

        private static IterationRepeatPars DetermineIterationRepeatPars(NeuralNetwork network)
        {
            int min = int.MaxValue;
            int max = int.MinValue;
            foreach (var bwRule in network.backwardLearningAlgorithms.Select(a => a.Rule).OfType<BackwardLearningRule>())
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

        [NonSerialized]
        bool initialized;

        [NonSerialized]
        ComputationHandle pushForwardHandle, popForwardHandle;

        [NonSerialized]
        Stack<double>[] stacks;

        #endregion

        #region Execute Logic

        #region State

        bool beginOfBatch;

        int recurrentIterations;

        #endregion

        #region Create Validation

        public Validation CreateValidation()
        {
            return new Validation(Network);
        }

        #endregion

        #region Initialize

        private void EnsureInitizalized()
        {
            if (!initialized)
            {
                Initialize();
                initialized = true;
            }
        }

        private void Initialize()
        {
            var network = Network;
            
            if (network.IsRecurrent && network.IsBackwardComputationRequired)
            {
                int nodeCount = network.Nodes.Count;
                var push = new ComputationBuilder<double>();
                var pop = new ComputationBuilder<double>();

                // Create stacks:
                stacks = new Stack<double>[nodeCount];

                for (int idx = 0; idx < nodeCount; idx++)
                {
                    // Create stack:
                    stacks[idx] = new Stack<double>();
                    string theStackDef = "var theStack = ((Stack<double>[])context)[" + idx + "]";
                    string theStack = "theStack";

                    var pushBlock = new ComputationBlock(idx);
                    var popBlock = new ComputationBlock(idx);

                    pushBlock.Add(theStackDef);
                    pushBlock.AddReference(typeof(Stack<double>));
                    popBlock.Add(theStackDef);
                    popBlock.AddReference(typeof(Stack<double>));

                    network.Nodes[idx].DefinePushForwardInformation(pushBlock, theStack);
                    network.Nodes[idx].DefinePopForwardInformation(popBlock, theStack);

                    push.AddBlock(pushBlock);
                    pop.AddBlock(popBlock);
                }

                Parallel.Invoke(
                    () => pushForwardHandle = push.Compile(network.ValueSpace, "PushForwardValues", stacks, @"c:\Temp\neuroflow\PushForwardValues.cs"),
                    () => popForwardHandle = pop.Compile(network.ValueSpace, "PopForwardValues", stacks, @"c:\Temp\neuroflow\PopForwardValues.cs"));
            }
        }

        #endregion

        #region Overrides

        // Batch Iterations:
        protected override BatchExecutionResult LockedDoBatchExcuteIterations(VectorFlowBatch<double> batch, Action<BatchExecutionResult> iterationCallback)
        {
            EnsureInitizalized();
            
            // This is where batch iterations begins

            var neuralBatch = batch as NeuralVectorFlowBatch;

            if (neuralBatch != null)
            {
                // Before iterations
                if (neuralBatch.ResetSchedule == ResetSchedule.BeforeExecution && Network.IsRecurrent)
                {
                    // Recurrent reset scheduled:
                    ResetAll();
                }
            }

            var result = base.LockedDoBatchExcuteIterations(batch, iterationCallback);

            if (neuralBatch != null)
            {
                // After iterations
                if (neuralBatch.ResetSchedule == ResetSchedule.AfterExecution && Network.IsRecurrent)
                {
                    // Recurrent reset scheduled:
                    ResetAll();
                }
            }

            return result;
        }

        // Batch Iteration:
        protected override double ExecuteBatchIteration(VectorFlowBatch<double> batch, double[] resultErrors)
        {
            // Begin of batch:
            beginOfBatch = true;

            if (Mode == TrainingMode.Unordered)
            {
                // Reset backward values, and clear error information:
                ResetBackwardValues();
            }
            else // Streamed
            {
                // Reset backward values, but preserve error information:
                ResetBackwardValuesButPreserveErrors();
            }

            double error = base.ExecuteBatchIteration(batch, resultErrors);

            Debug.Assert(batch.VectorFlows.Count == resultErrors.Length);

            // Batch done, iterate algos:
            BackwardBatchAlgoIteration(resultErrors.Length, error);

            // Batch done. Yeah.
            return error;
        }
 
        // A Flow:
        protected override double LockedExecuteVectorFlow(VectorFlow<double> vectorFlow)
        {
            // Do forward iteration first:
            ForwardAlgoIteration(beginOfBatch);

            if (Mode == TrainingMode.Unordered)
            {
                // Reset forward values:
                ResetForwardValues();

                // Reset backward errors and gradients:
                // If beginOfBatch == true, 
                // backward errors and gradients already reseted by a ResetBackwardValues() call
                if (!beginOfBatch)
                {
                    ResetBackwardErrorsAndGradients();
                }
            }

            beginOfBatch = false;

            // Begin of recurrent flow:
            recurrentIterations = 0;

            return base.LockedExecuteVectorFlow(vectorFlow);
        }

        // Register errors
        protected override void RegiterErrorDifference(int index, double desiredValue, double currentValue)
        {
            var network = Network;
            if (network.IsBackwardComputationRequired)
            {
                network.ErrorInputInterface.FastSet(index, desiredValue - currentValue);
            }
        }

        // Each vector iteration:
        protected override void Iteration(int iteration, int numberOfIterations)
        {
            base.Iteration(iteration, numberOfIterations);

            var network = Network;
            if (network.IsBackwardComputationRequired && network.IsRecurrent)
            {
                // Track info required for Recurrent BP.
                PushForwardInformation();

                recurrentIterations++;
            }
        }

        // Backpropagate
        protected override double? ComputeError(double?[] desiredOutputVector)
        {
            double? error = base.ComputeError(desiredOutputVector);

            if (error == null) return null;

            var network = Network;
            if (network.IsBackwardComputationRequired)
            {
                // Errors registered, backpropagate:
                if (network.IsRecurrent)
                {
                    for (int iterationIndex = 0; iterationIndex < recurrentIterations; iterationIndex++)
                    {
                        bool last = iterationIndex == recurrentIterations - 1;

                        PopForwardInformation();

                        network.LockedBackwardIteration(last ? BackwardComputationMode.RecurrentLastStep : BackwardComputationMode.Recurrent);

                        // Set Errors to 0.
                        if (iterationIndex == 0 && !last)
                        {
                            ZeroErrorInterface();
                        }
                    }

                    recurrentIterations = 0;
                }
                else
                {
                    network.LockedBackwardIteration(BackwardComputationMode.FeedForward);
                }
            }

            BackwardStochasticAlgoIteration(error.Value);

            return error;
        }

        #endregion

        #region Helpers

        private void PushForwardInformation()
        {
            Debug.Assert(Network.IsBackwardComputationRequired && Network.IsRecurrent && stacks != null);

            pushForwardHandle.Run();
        }

        private void PopForwardInformation()
        {
            Debug.Assert(Network.IsBackwardComputationRequired && Network.IsRecurrent && stacks != null);

            popForwardHandle.Run();
        }

        private void ZeroErrorInterface()
        {
            var network = Network;

            Debug.Assert(network.IsBackwardComputationRequired && network.IsRecurrent);

            network.ErrorInputInterface.Zero();
        }

        private void ResetBackwardErrorsAndGradients()
        {
            var network = Network;
            if (network.IsBackwardComputationRequired && network.IsRecurrent)
            {
                Parallel.Invoke(
                    () => network.LockedResetErrors(),
                    () => network.LockedResetGradients());
            }
        }

        private void ResetForwardValues()
        {
            var network = Network;
            if (network.IsRecurrent)
            {
                network.LockedReset();
            }
        }

        private void ResetBackwardValues()
        {
            var network = Network;
            if (network.IsBackwardComputationRequired)
            {
                if (network.IsRecurrent)
                {
                    Parallel.Invoke(
                        () => network.LockedResetErrors(),
                        () => network.LockedResetGradients(),
                        () => network.LockedResetGradientSums());
                }
                else
                {
                    network.LockedResetGradientSums();
                }
            }
        }

        private void ResetBackwardValuesButPreserveErrors()
        {
            var network = Network;
            if (network.IsBackwardComputationRequired)
            {
                if (network.IsRecurrent)
                {
                    Parallel.Invoke(
                        () => network.LockedResetGradients(),
                        () => network.LockedResetGradientSums());
                }
                else
                {
                    network.LockedResetGradientSums();
                }
            }
        }

        private void ResetAll()
        {
            Parallel.Invoke(
                () => InitializeNewAlgoRun(),
                () =>
                {
                    ResetForwardValues();
                    ResetBackwardValues();
                });
        }

        private void InitializeNewAlgoRun()
        {
            var network = Network;
            if (network.learningAlgorithms.Length > 1)
            {
                Parallel.ForEach(network.learningAlgorithms, (algo) => algo.InitializeNewRun());
            }
            else if (network.learningAlgorithms.Length == 1)
            {
                network.learningAlgorithms[0].InitializeNewRun();
            }
        }

        private void ForwardAlgoIteration(bool isNewBatch)
        {
            var network = Network;
            if (network.forwardLearningAlgorithms.Length > 1)
            {
                Parallel.ForEach(network.forwardLearningAlgorithms, (algo) => algo.ForwardIteration(isNewBatch));
            }
            else if (network.forwardLearningAlgorithms.Length == 1)
            {
                network.forwardLearningAlgorithms[0].ForwardIteration(isNewBatch);
            }
        }

        private void BackwardBatchAlgoIteration(int batchSize, double averageError)
        {
            var network = Network;
            if (network.backwardLearningAlgorithms.Length > 1)
            {
                Parallel.ForEach(network.backwardLearningAlgorithms, (algo) => algo.BackwardIteration(true, batchSize, averageError));
            }
            else if (network.backwardLearningAlgorithms.Length == 1)
            {
                network.backwardLearningAlgorithms[0].BackwardIteration(true, batchSize, averageError);
            }
        }

        private void BackwardStochasticAlgoIteration(double averageError)
        {
            var network = Network;
            if (network.backwardLearningAlgorithms.Length > 1)
            {
                Parallel.ForEach(network.backwardLearningAlgorithms, (algo) => algo.BackwardIteration(false, 0, averageError));
            }
            else if (network.backwardLearningAlgorithms.Length == 1)
            {
                network.backwardLearningAlgorithms[0].BackwardIteration(false, 0, averageError);
            }
        }

        #endregion

        #endregion
    }
}
