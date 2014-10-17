using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Neuroflow.Core.ComputationAPI;
using System.Diagnostics;
using Neuroflow.Networks.Neural.Learning;
using System.Collections.ObjectModel;
using Neuroflow.Core;
using System.Threading.Tasks;

namespace Neuroflow.Networks.Neural
{
    public enum BackwardComputationMode
    {
        FeedForward, Recurrent, RecurrentLastStep
    }
    
    public class NeuralNetwork : ComputationalNetwork<NeuralNode, NeuralConnection, double>
    {
        #region Constrcut

        public NeuralNetwork(int inputInterfaceLength, int outputInterfaceLength)
            : base(inputInterfaceLength, outputInterfaceLength)
        {
            Contract.Requires(inputInterfaceLength >= 0);
            Contract.Requires(outputInterfaceLength >= 0);
        }

        public NeuralNetwork(Guid structuralUID, int inputInterfaceLength, int outputInterfaceLength)
            : base(structuralUID, inputInterfaceLength, outputInterfaceLength)
        {
            Contract.Requires(inputInterfaceLength >= 0);
            Contract.Requires(outputInterfaceLength >= 0);
        } 

        #endregion

        #region Props and Fields

        public ComputationInterface<double> ErrorInputInterface { get; private set; }

        ComputationHandle backwardFFHandle, backwardRHandle, backwardRLHandle;

        ResetDoubleValues resetErrors, resetGradients, resetGradientSums;

        ValueReference<double> valueOfOneReference;

        LinkedList<Tuple<LearningRule, LearningAlgorithm, IEnumerable<IHasLearningRules>>> learningAlgosToInit;

        internal LearningAlgorithm[] learningAlgorithms;

        public ReadOnlyCollection<LearningAlgorithm> LearningAlgorithms
        {
            get { return learningAlgorithms != null ? Array.AsReadOnly(learningAlgorithms) : null; }
        }

        internal LearningAlgorithm[] forwardLearningAlgorithms;

        public ReadOnlyCollection<LearningAlgorithm> ForwardLearningAlgorithms
        {
            get { return forwardLearningAlgorithms != null ? Array.AsReadOnly(forwardLearningAlgorithms) : null; }
        }

        internal LearningAlgorithm[] backwardLearningAlgorithms;

        public ReadOnlyCollection<LearningAlgorithm> BackwardLearningAlgorithms
        {
            get { return backwardLearningAlgorithms != null ? Array.AsReadOnly(backwardLearningAlgorithms) : null; }
        }

        public bool IsBackwardComputationRequired { get; private set; }

        #endregion

        #region Build

        protected override void NodesCreated()
        {
            base.NodesCreated();

            CollectLearningRulesAndAlgorithms();

            if (IsBackwardComputationRequired)
            {
                InitializeBackwardComputation();
            }
        }

        private void InitializeBackwardComputation()
        {
            // 1 constrant value
            valueOfOneReference = new ValueReference<double>(ValueSpace);

            int maxTimeFrame = nodes[nodes.Length - 1].TimeFrame;

            var inputConns = new SortedDictionary<int, List<NeuralConnection>>();

            for (int ii = 0; ii < InputInterfaceLength; ii++) inputConns[ii] = new List<NeuralConnection>();

            var outConns = new SortedDictionary<int, NeuralConnection>();

            int beginOfOutputIndex = MaxNodeIndex - OutputInterfaceLength + 1;

            var ffBuilder = !IsRecurrent ? new ComputationBuilder<double>() : null;
            var rBuilder = IsRecurrent ? new ComputationBuilder<double>() : null;
            var rlBuilder = IsRecurrent ? new ComputationBuilder<double>() : null;

            for (int nodeIndex = 0; nodeIndex < nodes.Length; nodeIndex++)
            {
                var node = nodes[nodeIndex];

                foreach (NeuralConnection c in node.LowerConnections)
                {
                    if (!c.IsBackwardInitialized)
                    {
                        c.InitializeBackward(IsRecurrent, null);

                        if (c.Index.LowerNodeIndex >= beginOfOutputIndex)
                        {
                            outConns[c.Index.LowerNodeIndex - beginOfOutputIndex] = c;
                        }
                    }
                }

                if (node.IsOpeartionNode)
                {
                    if (node.UpperConnections.Count != 0)
                    {
                        int evidx = ValueSpace.Declare();
                        foreach (NeuralConnection c in node.UpperConnections)
                        {
                            if (!c.IsBackwardInitialized)
                            {
                                c.InitializeBackward(IsRecurrent, evidx);

                                if (c.Index.UpperNodeIndex < InputInterfaceLength)
                                {
                                    RegisterInputConn(inputConns, c);
                                }
                            }
                        }
                    }
                }
                else
                {
                    foreach (NeuralConnection c in node.UpperConnections)
                    {
                        if (!c.IsBackwardInitialized)
                        {
                            c.InitializeBackward(IsRecurrent, null);

                            if (c.Index.UpperNodeIndex < InputInterfaceLength)
                            {
                                RegisterInputConn(inputConns, c);
                            }
                        }
                    }
                }

                node.InitializeBackward(IsRecurrent, valueOfOneReference);

                if (!IsRecurrent)
                {
                    Debug.Assert(ffBuilder != null);
                    
                    var bwFFBlock = node.GetBackwardComputationBlock(BackwardComputationMode.FeedForward);

                    if (bwFFBlock != null)
                    {
                        bwFFBlock.TimeFrame = maxTimeFrame - node.TimeFrame;

                        ffBuilder.AddBlock(bwFFBlock);
                    }
                }
                else
                {
                    Debug.Assert(rBuilder != null);
                    Debug.Assert(rlBuilder != null);

                    var bwRBlock = node.GetBackwardComputationBlock(BackwardComputationMode.Recurrent);

                    if (bwRBlock != null)
                    {
                        bwRBlock.TimeFrame = maxTimeFrame - node.TimeFrame;

                        rBuilder.AddBlock(bwRBlock);
                    }

                    var bwRLBlock = node.GetBackwardComputationBlock(BackwardComputationMode.RecurrentLastStep);

                    if (bwRLBlock != null)
                    {
                        bwRLBlock.TimeFrame = maxTimeFrame - node.TimeFrame;

                        rlBuilder.AddBlock(bwRLBlock);
                    }
                }
            }

            Parallel.Invoke(
                () => ErrorInputInterface = new ComputationInterface<double>(SyncRoot, ValueSpace, outConns.Values.Select(c => c.ErrorValue.ValueIndex)),
                () => resetGradientSums = BuildResetHandle(GetAllItems().Cast<IResetableNeuralNetworkItem>().SelectMany(i => i.GetResetGradientSumAffectedIndexes())),
                () =>
                {
                    if (!IsRecurrent)
                    {
                        Debug.Assert(ffBuilder != null);

                        backwardFFHandle = ffBuilder.Compile(ValueSpace, "FFBackward");
                    }
                    else
                    {
                        Debug.Assert(rBuilder != null);
                        Debug.Assert(rlBuilder != null);

                        Parallel.Invoke(
                            () => resetErrors = BuildResetHandle(GetAllItems().Cast<IResetableNeuralNetworkItem>().SelectMany(i => i.GetResetErrorAffectedIndexes())),
                            () => resetGradients = BuildResetHandle(GetAllItems().Cast<IResetableNeuralNetworkItem>().SelectMany(i => i.GetResetGradientAffectedIndexes())),
                            () => backwardRHandle = rBuilder.Compile(ValueSpace, "RBackward"),
                            () => backwardRLHandle = rlBuilder.Compile(ValueSpace, "RLBackward"));
                    }
                });
        }

        private void RegisterInputConn(SortedDictionary<int, List<NeuralConnection>> inputConns, NeuralConnection c)
        {
            int index = c.Index.UpperNodeIndex;
            inputConns[index].Add(c);
        }

        #endregion

        #region After Built

        protected override void Built()
        {
            base.Built();

            if (IsBackwardComputationRequired)
            {
                valueOfOneReference.Value = 1.0;
            }

            InitializeLearningRulesAndAlgorithms();
        }

        #endregion

        #region Learning Rules and Algos

        private void CollectLearningRulesAndAlgorithms()
        {
            var rulesAndOwners = from hla in GetAllItems().OfType<IHasLearningRules>()
                                 from rule in hla.LearningRules
                                 select new
                                 {
                                     Owner = hla,
                                     Rule = rule
                                 };

            var groups = from item in rulesAndOwners
                         group item by item.Rule into g
                         select g;

            learningAlgosToInit = new LinkedList<Tuple<LearningRule, LearningAlgorithm, IEnumerable<IHasLearningRules>>>();
            foreach (var ruleItem in groups)
            {
                var rule = ruleItem.Key;
                var algo = (LearningAlgorithm)Activator.CreateInstance(rule.AlgorithmType);
                if (algo.BackwardIterationMode == BackwardIterationMode.EnabledAndBackpropagate) IsBackwardComputationRequired = true;
                learningAlgosToInit.AddLast(Tuple.Create(rule, algo, ruleItem.Select(i => i.Owner)));
            }
        }

        private void InitializeLearningRulesAndAlgorithms()
        {
            if (learningAlgosToInit != null)
            {
                try
                {
                    var algos = new LinkedList<LearningAlgorithm>();
                    foreach (var item in learningAlgosToInit)
                    {
                        var rule = item.Item1;
                        var algo = item.Item2;
                        var owners = item.Item3;
                        InitializeLearningAlgorithm(rule, algo, owners);
                        algos.AddLast(algo);
                    }
                    learningAlgorithms = algos.ToArray();
                    forwardLearningAlgorithms = learningAlgorithms.Where(a => a.WantForwardIteration).ToArray();
                    backwardLearningAlgorithms = learningAlgorithms.Where(a => a.BackwardIterationMode != BackwardIterationMode.Disabled).ToArray();
                }
                finally
                {
                    learningAlgosToInit = null;
                }
            }
        }

        private void InitializeLearningAlgorithm(LearningRule rule, LearningAlgorithm algo, IEnumerable<IHasLearningRules> owners)
        {
            var inputs = owners.Select(o => o.InputValueIndexes).ToArray();
            var outputs = owners.Select(o => o.OutputValueIndex).ToArray();

            algo.Initialize(rule, ValueSpace, inputs, outputs);
        }

        #endregion

        #region Backward Iteration

        public void BackwardIteration(BackwardComputationMode mode)
        {
            lock (SyncRoot)
            {
                LockedBackwardIteration(mode);
            }
        }

        public void LockedBackwardIteration(BackwardComputationMode mode)
        {
            switch (mode)
            {
                case BackwardComputationMode.FeedForward:
                    BackwardFFIteration();
                    break;
                case BackwardComputationMode.Recurrent:
                    BackwardRIteration();
                    break;
                case BackwardComputationMode.RecurrentLastStep:
                    BackwardRLIteration();
                    break;
            }
        }

        private void BackwardFFIteration()
        {
            if (backwardFFHandle == null) throw GetHasNotBuiltEx();
            backwardFFHandle.Run();
        }

        private void BackwardRIteration()
        {
            if (backwardRHandle == null) throw GetHasNotBuiltEx();
            backwardRHandle.Run();
        }

        private void BackwardRLIteration()
        {
            if (backwardRLHandle == null) throw GetHasNotBuiltEx();
            backwardRLHandle.Run();
        }

        #endregion

        #region Reset

        protected ResetDoubleValues BuildResetHandle(IEnumerable<int> indexes)
        {
            return new ResetDoubleValues(ValueSpace, indexes.ToArray());
        }

        protected override IReset CreateReset()
        {
            return BuildResetHandle(Connections.Select(c => c.InputValue.ValueIndex));
        }

        public void ResetGradientSums()
        {
            lock (SyncRoot)
            {
                LockedResetGradientSums();
            }
        }

        public void LockedResetGradientSums()
        {
            if (resetGradientSums == null) throw GetHasNotBuiltEx();

            resetGradientSums.Reset();
        }

        public void ResetErrors()
        {
            lock (SyncRoot)
            {
                LockedResetErrors();
            }
        }

        public void LockedResetErrors()
        {
            if (resetErrors == null) throw GetHasNotBuiltEx();

            resetErrors.Reset();
        }

        public void ResetGradients()
        {
            lock (SyncRoot)
            {
                LockedResetGradients();
            }
        }

        public void LockedResetGradients()
        {
            if (resetGradients == null) throw GetHasNotBuiltEx();

            resetGradients.Reset();
        }

        #endregion
    }
}
