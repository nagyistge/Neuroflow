using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using NeoComp.Learning;
using NeoComp.Computations2;
using System.Diagnostics.Contracts;
using NeoComp.Core;

namespace NeoComp.Networks.Computational.Neural
{
    [Serializable]
    [DataContract(IsReference = true, Namespace = NeoComp.xmlns, Name = "neuralLearningScriptBatchExec")]
    public sealed class NeuralLearningScriptBatchExecution : LearningScriptBatchExecution, IReset, ISynchronized
    {
        public NeuralLearningScriptBatchExecution(NeuralNetwork network, IterationRepeatPars repeatPars = null, double errorScale = 0.5)
            : base((IComputationUnit<double>)network, errorScale, repeatPars)
        {
            Contract.Requires(network != null);
            Contract.Requires(errorScale > 0.0);
        }

        #region Properties and Fields

        [NonSerialized]
        bool isNewBatchState;

        [NonSerialized]
        bool backwardPhaseEnabled;

        [NonSerialized]
        IBackwardConnection[][] bwOutputConns, bwInputConns;

        [NonSerialized]
        BackwardPropagatorHelper[] backwardPropagators;

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
            var bwas = new Dictionary<Tuple<Type, int>, LearningAlgorithm>();
            var fwas = new Dictionary<Tuple<Type, int>, LearningAlgorithm>();
            var entries = Network.EntryArray;
            foreach (var entry in entries)
            {
                var ln = entry.NodeEntry.Node as ILearningConnection;
                if (ln != null) AddLearningAlgo(ln, fwas, bwas);
                foreach (var uce in entry.UpperConnectionEntryArray)
                {
                    var lc = uce.Connection as ILearningConnection;
                    if (lc != null) AddLearningAlgo(lc, fwas, bwas);
                }
            }
            backwardLearningAlgos = bwas.Values.ToArray();
            forwardLearningAlgos = fwas.Values.ToArray();
            foreach (var bwa in backwardLearningAlgos) bwa.EndInit();
            foreach (var fwa in forwardLearningAlgos) fwa.EndInit();
            InitializeNewAlgoRun();
        }

        private void AddLearningAlgo(ILearningConnection connection, Dictionary<Tuple<Type, int>, LearningAlgorithm> fwAlgos, Dictionary<Tuple<Type, int>, LearningAlgorithm> bwAlgos)
        {
            if (connection.LearningRules == null) return;
            foreach (var rule in connection.LearningRules)
            {
                if (rule.IsEnabled)
                {
                    if (connection is IBackwardConnection && rule is IBackwardRule)
                    {
                        AddLearningAlgo(connection, rule, bwAlgos);
                        var bwRule = (IBackwardRule)rule;
                        if (bwRule.WantForwardIteration) AddLearningAlgo(connection, rule, fwAlgos);
                        if (bwRule.WantGradientInformation) backwardPhaseEnabled = true;
                    }
                    else
                    {
                        AddLearningAlgo(connection, rule, fwAlgos);
                    }
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
            }
            algo.AddConnection(connection, rule);
        }

        #endregion

        #region Backward Setup

        private void EnsureBackwardSetup()
        {
            if (bwInputConns == null && bwOutputConns == null) SetupBackwardConnections();
            if (backwardPropagators == null) SetupBackwardPropagators();
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

        private void SetupBackwardPropagators()
        {
            var list = new LinkedList<BackwardPropagatorHelper>();
            var entries = Network.EntryArray;
            for (int idx = entries.Length - 1; idx >= 0; idx--)
            {
                var entry = entries[idx];
                var prop = entry.NodeEntry.Node as IBackwardPropagator;
                if (prop != null)
                {
                    list.AddLast(
                        new BackwardPropagatorHelper(prop, entry.UpperConnectionEntryArray, entry.LowerConnectionEntryArray));
                }
            }
            backwardPropagators = list.ToArray();
        }

        #endregion

        #region Script

        private void EnsureScriptRun()
        {
            EnsureAlgoSetup();
            EnsureBackwardSetup();
        }

        // Batch:
        protected override ScriptBatchExecutionResult GuardedExecute(LearningScriptBatch scriptBatch)
        {
            EnsureScriptRun();
            
            isNewBatchState = true;
            var result = base.GuardedExecute(scriptBatch);

            // Batch done, iterate algos:
            foreach (var bwa in backwardLearningAlgos) bwa.BackwardIteration(true, result.AverageError);

            // Batch done. Yeah.
            return result;
        }

        // An entry:
        protected override double Excute(IEnumerable<ComputationScriptEntry<double>> scriptEntries)
        {
            // Do forward iteration first:
            foreach (var fwa in forwardLearningAlgos) fwa.ForwardIteration(isNewBatchState);
            
            double error = base.Excute(scriptEntries);
            isNewBatchState = false;
            return error;
        }

        // An error vector:
        protected override double ComputeError(double?[] desiredOutputVector)
        {
            double error = base.ComputeError(desiredOutputVector);

            if (backwardPhaseEnabled)
            {
                // Errors registered, backpropagate:
                foreach (var prop in backwardPropagators) prop.Propagate(isNewBatchState);

                // Iterate algos:
                foreach (var bwa in backwardLearningAlgos) bwa.BackwardIteration(false, error);
            }

            return error;
        }

        // Error difference:
        protected override void RegiterErrorDifference(int index, double difference)
        {
            if (backwardPhaseEnabled)
            {
                foreach (var boc in bwOutputConns[index]) boc.BackwardValues.AddNext(difference, boc.InputValue, isNewBatchState);
            }
        }

        #endregion

        #region Reset

        public void Reset()
        {
            lock (SyncRoot)
            {
                InitializeNewAlgoRun();
            }
        }

        private void InitializeNewAlgoRun()
        {
            foreach (var fwa in forwardLearningAlgos) fwa.InitializeNewRun();
            foreach (var bwa in backwardLearningAlgos) bwa.InitializeNewRun();
        }

        #endregion
    }
}
