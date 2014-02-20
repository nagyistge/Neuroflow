using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using NeoComp.Learning;
using NeoComp.Computations;
using System.Diagnostics.Contracts;
using NeoComp.Core;

namespace NeoComp.Networks.Computational.Neural
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = NeoComp.xmlns, Name = "neuralLearningExec")]
    public sealed class LearningExecution : ScriptBatchExecution, ISynchronized
    {
        public LearningExecution(NeuralNetwork network, IterationRepeatPars repeatPars = null, double errorScale = 0.5)
            : base((IComputationUnit<double>)network, errorScale, repeatPars)
        {
            Contract.Requires(network != null);
            Contract.Requires(errorScale > 0.0);
        }

        #region Properties and Fields

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

        new public NeuralNetwork ComputationUnit
        {
            get { return (NeuralNetwork)base.ComputationUnit; }
        }

        public NeuralNetwork Network
        {
            get { return ComputationUnit; }
        }

        public SyncContext SyncRoot
        {
            get { return Network.SyncRoot; }
        }

        bool IsRecurrent
        {
            get { return !Network.IsFeedForward; }
        }

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
            InitializeNewAlgoRun(AlgoInitializationMode.Startup);
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

        #region Script

        private void EnsureScriptRun()
        {
            EnsureAlgoSetup();
            EnsureBackwardSetup();
        }

        protected override ScriptBatchExecutionResult BatchExecution(ScriptBatch scriptBatch, Action<ScriptBatchExecutionResult> iterationCallback)
        {
            InitializeNewAlgoRun(AlgoInitializationMode.BatchStart);
            var result = base.BatchExecution(scriptBatch, iterationCallback);
            return result;
        }

        // Batch:
        protected override double ExecuteBatch(ScriptBatch scriptBatch, double[] resultErrors)
        {
            EnsureScriptRun();

            // Begin of batch:
            isNewBatchState = true;

            // Reset backward values:
            ResetBackwardValues();

            double error = base.ExecuteBatch(scriptBatch, resultErrors);

            // Batch done, iterate algos:
            foreach (var bwa in backwardLearningAlgos) bwa.BackwardIteration(true, error);

            // Batch done. Yeah.
            return error;
        }

        // An entry:
        protected override double Excute(IEnumerable<ComputationScriptEntry<double>> scriptEntries)
        {
            // Do forward iteration first:
            foreach (var fwa in forwardLearningAlgos) fwa.ForwardIteration(isNewBatchState);

            // Reset backward errors:
            if (!isNewBatchState) ResetBackwardErrors();

            isNewBatchState = false;

            // Track count to 0:
            infoTrackCountSoFar = 0;
            
            return base.Excute(scriptEntries);
        }

        // Track Iteration Information:
        protected override void Iteration(int iteration, int numberOfIterations)
        {
            base.Iteration(iteration, numberOfIterations);

            if (doBackpropagation)
            {
                // Track info required for Recurrent BP.
                if (IsRecurrent)
                {
                    foreach (var bp in backpropagators) bp.TrackForwardInformation();

                    infoTrackCountSoFar++;
                }
            }
        }

        // An error vector to backpropagate:
        protected override double ComputeError(double?[] desiredOutputVector)
        {
            double error = base.ComputeError(desiredOutputVector);

            if (doBackpropagation)
            {
                // Errors registered, backpropagate:
                if (IsRecurrent)
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

            // Iterate algos:
            foreach (var bwa in backwardLearningAlgos)
            {
                if (bwa.BackwardIterationMode == BackwardIterationMode.Enabled || bwa.BackwardIterationMode == BackwardIterationMode.BackPropagation)
                {
                    bwa.BackwardIteration(false, error);
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

        protected override void Reset(ResetMode mode)
        {
            base.Reset(mode);
            if (mode == ResetMode.Reinitialize)
            {
                InitializeNewAlgoRun(AlgoInitializationMode.Startup);
                ResetBackwardValues();
            }
        }

        private void ResetBackwardErrors()
        {
            if (doBackpropagation && IsRecurrent)
            {
                foreach (var bwv in bwValues) bwv.ResetErros();
            }
        }

        private void ResetBackwardValues()
        {
            if (doBackpropagation)
            {
                foreach (var bwv in bwValues) bwv.Reset();
            }
        }

        private void InitializeNewAlgoRun(AlgoInitializationMode mode)
        {
            if (forwardLearningAlgos != null) foreach (var fwa in forwardLearningAlgos) fwa.InitializeNewRun(mode);
            if (backwardLearningAlgos != null) foreach (var bwa in backwardLearningAlgos) bwa.InitializeNewRun(mode);
        }

        #endregion
    }
}
