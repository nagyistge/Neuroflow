using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Computations;
using NeoComp.Core;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace NeoComp.Networks.Computational.Neural
{
    public sealed class Learning : SynchronizedObject, 
        IUnsupervisedComputation<double>, 
        ISupervisedComputation<double>, 
        IComputationalUnit<double?>, 
        IComputationalUnit<double>,
        IReset
    {
        #region Backward Propagator Class

        sealed class BackwardPropagator
        {
            internal BackwardPropagator(IBackwardPropagator propagator, ConnectionEntry<ComputationalConnection<double>>[] inputs, ConnectionEntry<ComputationalConnection<double>>[] outputs)
            {
                Contract.Requires(propagator != null);
                Contract.Requires(inputs != null);
                Contract.Requires(outputs != null);

                Propagator = propagator;
                InputEntries = (from e in inputs
                                let bc = e.Connection as IBackwardConnection
                                where bc != null
                                select new BackwardConnectionEntry(e.Index, bc)).ToArray();
                OutputEntries = (from e in outputs
                                 let bc = e.Connection as IBackwardConnection
                                 where bc != null
                                 select new BackwardConnectionEntry(e.Index, bc)).ToArray();
            }
            
            internal IBackwardPropagator Propagator { get; private set;}

            internal BackwardConnectionEntry[] InputEntries { get; private set; }

            internal BackwardConnectionEntry[] OutputEntries { get; private set; }

            internal void Propagate(bool isNewBatch)
            {
                Propagator.BackPropagate(OutputEntries, InputEntries, isNewBatch);
            }
        }

        #endregion

        #region Constructor

        public Learning(NeuralNetwork network, int repetitionCount = 1, int numberOfIterations = 1)
            : base(network.SyncRoot)
        {
            Contract.Requires(network != null);
            Contract.Requires(numberOfIterations >= 1);
            Contract.Requires(repetitionCount >= 1);

            Network = network;
            computation = new MatrixComputation<double>(numberOfIterations);
            RepetitionCount = repetitionCount;

            SetupAlgos();
        }

        #endregion

        #region Properties and Fields

        bool backwardPhaseEnabled;

        MatrixComputation<double> computation;

        IBackwardConnection[][] bwOutputConns, bwInputConns;

        BackwardPropagator[] backwardPropagators;

        LearningAlgorithm[] forwardLearningAlgos, backwardLearningAlgos;

        public NeuralNetwork Network { get; private set; }

        public int NumberOfIterations
        {
            get { return computation.NumberOfIterations; }
        }

        public int RepetitionCount { get; private set; }

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
            var list = new LinkedList<BackwardPropagator>();
            var entries = Network.EntryArray;
            for (int idx = entries.Length - 1; idx >= 0; idx--)
            {
                var entry = entries[idx];
                var prop = entry.NodeEntry.Node as IBackwardPropagator;
                if (prop != null)
                {
                    list.AddLast(
                        new BackwardPropagator(prop, entry.UpperConnectionEntryArray, entry.LowerConnectionEntryArray));
                }
            }
            backwardPropagators = list.ToArray();
        }

        #endregion

        #region Algos

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

        #region Learn

        private void ComputeOutput(bool learnOn, Matrix<double> input, out Matrix<double> output, Matrix<double> desiredOutput = null)
        {
            double mse = 0.0;

            ActionAfterRowVectorComputation<double> learnMethod = null;
            if (learnOn)
            {
                learnMethod = (computedVector, rowIndex) =>
                {
                    bool isNew = rowIndex == 0;
                    if (ForwardLearning(isNew) && desiredOutput != null)
                    {
                        var errorVector = new Vector<double>(computedVector.ComputeDifferenceInternal(desiredOutput.ItemArray[rowIndex]));
                        double vmse = ComputeMSE(errorVector);
                        BackwardLearning(errorVector, isNew, vmse);
                        mse += vmse;
                    }
                };
            }

            lock (SyncRoot)
            {
                output = null;
                int count = learnOn ? RepetitionCount : 1;
                for (int i = 0; i < count; i++)
                {
                    output = computation.Compute(
                        Network,
                        input,
                        learnMethod);

                    if (learnOn && desiredOutput != null)
                    {
                        mse /= (double)input.ItemArray.Length;
                        foreach (var bwa in backwardLearningAlgos)
                        {
                            bwa.BackwardIteration(true, mse);
                        }
                    }
                }
            }
        }

        private double ComputeMSE(Vector<double> errorVector)
        {
            double error = 0.0;
            var values = errorVector.ItemArray;
            foreach (var value in values)
            {
                if (value.HasValue)
                {
                    double nv = value.Value * 0.5;
                    error += nv * nv;
                }
            }
            return error / (double)values.Length;
        }

        private bool ForwardLearning(bool isNewBatch)
        {
            bool done = true;
            foreach (var fwa in forwardLearningAlgos) done = done && fwa.ForwardIteration(isNewBatch);
            return done;
        }

        private void BackwardLearning(Vector<double> errorVector, bool isNewBatch, double mse)
        {
            if (backwardPhaseEnabled)
            {
                EnsureBackwardSetup();
                SetErrorsToBackwardOutputConns(errorVector, isNewBatch);
                BackwardPropagate(isNewBatch);
            }
            
            foreach (var bwa in backwardLearningAlgos) bwa.BackwardIteration(false, mse);
        }

        private void BackwardPropagate(bool isNewBatch)
        {
            foreach (var prop in backwardPropagators) prop.Propagate(isNewBatch);
        }

        private void SetErrorsToBackwardOutputConns(Vector<double> errorVector, bool isNewBatch)
        {
            var values = errorVector.ItemArray;
            for (int idx = 0; idx < values.Length; idx++)
            {
                SetErrorToBackwardOutputConns(bwOutputConns[idx], values[idx], isNewBatch);
            }
        }

        private void SetErrorToBackwardOutputConns(IBackwardConnection[] outConns, double? error, bool isNewBatch)
        {
            if (error.HasValue)
            {
                foreach (var boc in outConns) boc.BackwardValues.AddNext(error.Value, boc.InputValue, isNewBatch);
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

        #region Computational Interfaces

        double ISupervisedComputation<double>.Scale
        {
            get { return 0.5; }
        }

        void IUnsupervisedComputation<double>.Compute(Matrix<double> input, out Matrix<double> output, bool doAdjustments)
        {
            ComputeOutput(doAdjustments, input, out output);
        }

        void ISupervisedComputation<double>.Compute(Matrix<double> input, Matrix<double> desiredOutput, out Matrix<double> output, bool doAdjustments)
        {
            ComputeOutput(doAdjustments, input, out output, desiredOutput);
        }

        void IComputationalUnit<double?>.ComputeOutput(System.Threading.CancellationToken? cancellationToken)
        {
            ((IComputationalUnit<double?>)Network).ComputeOutput(cancellationToken);
        }

        IComputationalInterface<double?> IComputationalUnit<double?>.InputInterface
        {
            get { return ((IComputationalUnit<double?>)Network).InputInterface; }
        }

        IComputationalInterface<double?> IComputationalUnit<double?>.OutputInterface
        {
            get { return ((IComputationalUnit<double?>)Network).OutputInterface; }
        }

        void IComputationalUnit<double>.ComputeOutput(System.Threading.CancellationToken? cancellationToken)
        {
            ((IComputationalUnit<double>)Network).ComputeOutput(cancellationToken);
        }

        IComputationalInterface<double> IComputationalUnit<double>.InputInterface
        {
            get { return ((IComputationalUnit<double>)Network).InputInterface; }
        }

        IComputationalInterface<double> IComputationalUnit<double>.OutputInterface
        {
            get { return ((IComputationalUnit<double>)Network).OutputInterface; }
        } 

        #endregion
    }
}
