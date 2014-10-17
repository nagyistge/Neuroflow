using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core.Optimizations;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using Neuroflow.Core.Optimizations.NeuralNetworks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Neuroflow.ComponentModel;

namespace Neuroflow.Core.NeuralNetworks.Learning
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = xmlns.Neuroflow)]
    public sealed class StreamedTraining : Training
    {
        public StreamedTraining(
            [Required]
            [FreeDisplayName("Network")]
            [Category(PropertyCategories.Structure)]
            NeuralNetwork network)
            : base(network, null)
        {
            Contract.Requires(network != null);
        }

        public override bool IsStreamed
        {
            get { return true; }
        }
    }

    [Serializable]
    [DataContract(IsReference = true, Namespace = xmlns.Neuroflow)]
    public sealed class UnorderedTraining : Training
    {
        public UnorderedTraining(
            [Required]
            [FreeDisplayName("Network")]
            [Category(PropertyCategories.Structure)]
            NeuralNetwork network,
            [FreeDisplayName("Repeat Parameters")]
            [Category(PropertyCategories.Behavior)]
            IterationRepeatPars repeatPars = null)
            : base(network, repeatPars)
        {
            Contract.Requires(network != null);
        }

        public override bool IsStreamed
        {
            get { return false; }
        }
    }
    
    [Serializable]
    [DataContract(IsReference = true, Namespace = xmlns.Neuroflow, Name = "neuralTraining")]
    public abstract class Training : NeuralBatchExecution
    {
        #region Construct

        protected Training(NeuralNetwork network, IterationRepeatPars repeatPars)
            : base(network, repeatPars)
        {
            Contract.Requires(network != null);
        }

        #endregion

        #region Properties and Fields

        public abstract bool IsStreamed { get; }

        [NonSerialized]
        bool isNewBatchState;

        [NonSerialized]
        bool doBackpropagation;

        [NonSerialized]
        int infoTrackCountSoFar;

        [NonSerialized]
        IBackwardConnection[][] bwOutputConns, bwInputConns;

        [NonSerialized]
        BackwardValues[] bwValues;

        [NonSerialized]
        Backpropagator[] backpropagators;

        [NonSerialized]
        LearningAlgorithm[] forwardLearningAlgos, backwardLearningAlgos;

        #endregion

        #region Algos

        private void EnsureAlgoSetup()
        {
            if (backwardLearningAlgos == null || forwardLearningAlgos == null)
            {
                SetupAlgos();
            }
        }

        private void SetupAlgos()
        {
            var algos = new Dictionary<Tuple<Type, int>, LearningAlgorithm>();
            var entries = Network.EntryArray;
            foreach (var entry in entries)
            {
                var ln = entry.NodeEntry.Node as ILearningConnection;
                if (ln != null) AddLearningAlgo(ln, algos);
                foreach (var uce in entry.UpperConnectionEntryArray)
                {
                    var lc = uce.Connection as ILearningConnection;
                    if (lc != null) AddLearningAlgo(lc, algos);
                }
            }
            backwardLearningAlgos = algos.Values
                .Where(a => a.BackwardIterationMode == BackwardIterationMode.Enabled ||
                    a.BackwardIterationMode == BackwardIterationMode.BackPropagation)
                .ToArray();
            forwardLearningAlgos = algos.Values.Where(a => a.WantForwardIteration).ToArray();
            foreach (var bwa in backwardLearningAlgos) bwa.EndInit();
            foreach (var fwa in forwardLearningAlgos) fwa.EndInit();
            InitializeNewAlgoRun();
        }

        private void AddLearningAlgo(ILearningConnection connection, Dictionary<Tuple<Type, int>, LearningAlgorithm> algos)
        {
            if (connection.LearningRules == null) return;
            foreach (var rule in connection.LearningRules)
            {
                if (rule.IsEnabled)
                {
                    AddLearningAlgo(connection, rule, algos);
                }
            }
        }

        private void AddLearningAlgo(ILearningConnection connection, ILearningRule rule, Dictionary<Tuple<Type, int>, LearningAlgorithm> algos)
        {
            var key = Tuple.Create(rule.AlgorithmType, rule.GroupID);
            LearningAlgorithm algo;
            if (!algos.TryGetValue(key, out algo))
            {
                algo = (LearningAlgorithm)Activator.CreateInstance(rule.AlgorithmType);
                algo.BeginInit();
                algos.Add(key, algo);
                if (algo.BackwardIterationMode == BackwardIterationMode.BackPropagation) doBackpropagation = true;
            }
            algo.AddConnection(connection, rule);
        }

        #endregion

        #region Backward Setup

        private void EnsureBackwardSetup()
        {
            if (doBackpropagation)
            {
                if (bwInputConns == null && bwOutputConns == null) SetupBackwardConnections();

                if (backpropagators == null) SetupBackpropagators();
            }
        }

        private void SetupBackwardConnections()
        {
            var bwInput = Enumerable.Range(0, Network.InputInterface.Length).Select(idx => new LinkedList<IBackwardConnection>()).ToList();
            var bwOutput = Enumerable.Range(0, Network.OutputInterface.Length).Select(idx => new LinkedList<IBackwardConnection>()).ToList();

            foreach (var ice in Network.GetInputConnectionEntries().Where(e => e.Connection is IBackwardConnection))
            {
                bwInput[ice.Index.UpperNodeIndex].AddLast(((IBackwardConnection)ice.Connection));
            }

            int endIdx = Network.MaxEntryIndex - Network.OutputInterface.Length;
            foreach (var oce in Network.GetOutputConnectionEntries().Where(e => e.Connection is IBackwardConnection))
            {
                bwOutput[oce.Index.LowerNodeIndex - endIdx - 1].AddLast(((IBackwardConnection)oce.Connection));
            }

            bwInputConns = ToArray(bwInput);
            bwOutputConns = ToArray(bwOutput);

            var items = Network.GetItems();
            var cbwvals = items.ConnectionEntries.Select(e => e.Connection).OfType<IBackwardConnection>().Select(bwc => bwc.BackwardValues);
            var nbwvals = items.NodeEntries.Select(e => e.Node).OfType<IBackwardConnection>().Select(bwc => bwc.BackwardValues);
            bwValues = cbwvals.Concat(nbwvals).ToArray();
        }

        private IBackwardConnection[][] ToArray(List<LinkedList<IBackwardConnection>> list)
        {
            var result = new IBackwardConnection[list.Count][];
            for (int idx = 0; idx < result.Length; idx++)
            {
                result[idx] = list[idx].ToArray();
            }
            return result;
        }

        private void SetupBackpropagators()
        {
            var list = new LinkedList<Backpropagator>();
            var entries = Network.EntryArray;
            for (int idx = entries.Length - 1; idx >= 0; idx--)
            {
                var entry = entries[idx];
                var backNode = entry.NodeEntry.Node as IBackwardNode;
                if (backNode != null)
                {
                    var bp = backNode.CreateBackprogatator();
                    bp.Initialize(entry.UpperConnectionEntryArray, entry.LowerConnectionEntryArray);
                    list.AddLast(bp);
                }
            }
            backpropagators = list.ToArray();
        }

        #endregion

        #region Run

        public void EnsureScriptRun()
        {
            doBackpropagation = true;
            EnsureAlgoSetup();
            EnsureBackwardSetup();
        }

        public void BackwardIteration()
        {
            foreach (var bp in backpropagators) bp.Backpropagate();
        }

        protected override BatchExecutionResult BatchExecution(VectorFlowBatch<double> batch, Action<BatchExecutionResult> iterationCallback)
        {
            var neuralBatch = batch as NeuralBatch;
            if (batch != null)
            {
                // If completelly new training set execution begins:
                if (neuralBatch.ResetSchedule == TrainingResetSchedule.BeforeExecution && !Network.IsFeedForward)
                {
                    ResetForwardValues();
                    ResetBackwardValues(true);
                }
            }

            var result = base.BatchExecution(batch, iterationCallback);

            if (batch != null)
            {
                // If completelly new training set execution begins:
                if (neuralBatch.ResetSchedule == TrainingResetSchedule.AfterExecution && !Network.IsFeedForward)
                {
                    ResetForwardValues();
                    ResetBackwardValues(true);
                }
            }

            return result;
        }

        // Batch:
        protected override double ExecuteBatch(VectorFlowBatch<double> batch, double[] resultErrors)
        {
            EnsureScriptRun();

            if (!IsStreamed)
            {
                // Begin of batch:
                isNewBatchState = true;

                // Reset backward values, and clear error information:
                ResetBackwardValues(true);
            }
            else // Streamed
            {
                // Reset backward values, but preserve error information:
                ResetBackwardValues(false);
            }

            double error = base.ExecuteBatch(batch, resultErrors);

            // Batch done, iterate algos:
            foreach (var bwa in backwardLearningAlgos) bwa.BackwardIteration(true, error);

            // Batch done. Yeah.
            return error;
        }

        // An entry:
        protected override double Excute(VectorFlow<double> vectorFlow)
        {
            // Do forward iteration first:
            foreach (var fwa in forwardLearningAlgos) fwa.ForwardIteration(isNewBatchState);

            if (!IsStreamed)
            {
                // Reset forward values:
                ResetForwardValues();

                // Reset backward errors:
                if (!isNewBatchState)
                {
                    ResetBackwardErrors();
                }

                isNewBatchState = false;
            }

            // Track count to 0:
            infoTrackCountSoFar = 0;

            return base.Excute(vectorFlow);
        }

        // Track Iteration Information:
        protected override void Iteration(int iteration, int numberOfIterations)
        {
            base.Iteration(iteration, numberOfIterations);

            if (doBackpropagation && (!Network.IsFeedForward))
            {
                // Track info required for Recurrent BP.
                foreach (var bp in backpropagators) bp.TrackForwardInformation();

                infoTrackCountSoFar++;
            }
        }

        // An error vector to backpropagate:
        protected override double? ComputeError(double?[] desiredOutputVector)
        {
            double? error = base.ComputeError(desiredOutputVector);

            if (error == null) return null;

            if (doBackpropagation)
            {
                // Errors registered, backpropagate:
                if (!Network.IsFeedForward)
                {
                    for (int bpIdx = 0; bpIdx < infoTrackCountSoFar; bpIdx++)
                    {
                        foreach (var bp in backpropagators) bp.Backpropagate();

                        // Set Errors to 0.
                        if (bpIdx == 0 && bpIdx != infoTrackCountSoFar - 1)
                        {
                            for (int oidx = 0; oidx < bwOutputConns.Length; oidx++)
                            {
                                foreach (var boc in bwOutputConns[oidx])
                                {
                                    boc.BackwardValues.Set(0);
                                }
                            }
                        }
                    }
                    infoTrackCountSoFar = 0;
                }
                else
                {
                    foreach (var bp in backpropagators) bp.Backpropagate();
                }
            }

            foreach (var bwa in backwardLearningAlgos)
            {
                if (bwa.BackwardIterationMode == BackwardIterationMode.Enabled || bwa.BackwardIterationMode == BackwardIterationMode.BackPropagation)
                {
                    bwa.BackwardIteration(false, error.Value);
                }
            }

            return error;
        }

        protected override void RegiterErrorDifference(int index, double desiredValue, double currentValue)
        {
            if (doBackpropagation)
            {
                foreach (var boc in bwOutputConns[index])
                {
                    boc.BackwardValues.Set(desiredValue - currentValue);
                }
            }
        }

        #endregion

        #region Reset

        protected override void DoReset()
        {
            base.DoReset(); // Reset forward values.
            ResetBackwardValues(true);
            InitializeNewAlgoRun();
        }

        private void ResetForwardValues()
        {
            if (doBackpropagation && (!Network.IsFeedForward))
            {
                ResetComputationUnit();
            }
        }

        private void ResetBackwardErrors()
        {
            if (doBackpropagation && (!Network.IsFeedForward))
            {
                foreach (var bwv in bwValues) bwv.ResetLastError();
            }
        }

        private void ResetBackwardValues(bool resetLastError)
        {
            if (doBackpropagation)
            {
                foreach (var bwv in bwValues)
                {
                    bwv.ResetTracking();
                    if (resetLastError) bwv.ResetLastError();
                }
            }
        }

        private void InitializeNewAlgoRun()
        {
            if (forwardLearningAlgos != null) foreach (var fwa in forwardLearningAlgos) fwa.InitializeNewRun();
            if (backwardLearningAlgos != null) foreach (var bwa in backwardLearningAlgos) bwa.InitializeNewRun();
        }

        #endregion

        #region Validation Support

        public Validation CreateValidation()
        {
            return new Validation(Network);
        }

        #endregion
    }
}
