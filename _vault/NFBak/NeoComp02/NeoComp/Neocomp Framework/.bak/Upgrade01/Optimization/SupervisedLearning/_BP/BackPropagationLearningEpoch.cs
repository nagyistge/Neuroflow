using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks2.Computational.Neural;
using System.Diagnostics.Contracts;
using NeoComp.Computations;
using NeoComp.Networks2.Computational;
using NeoComp.Networks2;
using System.Diagnostics;

namespace NeoComp.Optimization.SupervisedLearning
{
    public sealed class BackPropagationLearningEpoch : AdjusterLearningEpoch
    {
        public BackPropagationLearningEpoch(BackPropagationRule rule, NeuralNetwork network, NeuralNetworkTest test = null)
            : base(network, test)
        {
            Contract.Requires(network != null);
            Contract.Requires(rule != null);

            Rule = rule;
        }

        OnlineNeuralNetworkTest onlineTest;

        BackPropagationNetworkEntryContext[] contexts;

        List<BackPropagationConnectionContext>[] outputs;

        public BackPropagationRule Rule { get; private set; }

        protected override void TestChanged()
        {
            onlineTest = new OnlineNeuralNetworkTest(CurrentTest, true);
        }

        protected override void InitializeNewRun()
        {
            Rule.Initialize();

            var conns = new Dictionary<ConnectionIndex, BackPropagationConnectionContext>();
            var contextList = new List<BackPropagationNetworkEntryContext>(Network.EntryArray.Length);
            int endIdx = Network.MaxEntryIndex - Network.OutputInterface.Length;
            outputs = Enumerable.Range(0, Network.OutputInterface.Length).Select(idx => new List<BackPropagationConnectionContext>()).ToArray();
            foreach (var entry in Network.EntryArray)
            {
                // Node:
                var nodeCtx = new BackPropgationNodeContext(Rule.CreateRuleContext(entry.NodeEntry.Node), entry.NodeEntry.Node);
                Rule.Initialize(nodeCtx.Node, nodeCtx.RuleContext);

                // Upper:
                var upperCtxs = new List<BackPropagationConnectionContext>(entry.UpperConnectionEntryArray.Length);
                foreach (var connEntry in entry.UpperConnectionEntryArray)
                {
                    BackPropagationConnectionContext connCtx;
                    if (!conns.TryGetValue(connEntry.Index, out connCtx))
                    {
                        connCtx = new BackPropagationConnectionContext(Rule.CreateRuleContext(connEntry.Connection), connEntry.Connection);
                        Rule.Initialize(connCtx.Connection, connCtx.RuleContext);
                        conns.Add(connEntry.Index, connCtx);
                    }
                    upperCtxs.Add(connCtx);
                }

                // Lower:
                var lowerCtxs = new List<BackPropagationConnectionContext>(entry.LowerConnectionEntryArray.Length);
                foreach (var connEntry in entry.LowerConnectionEntryArray)
                {
                    BackPropagationConnectionContext connCtx;
                    if (!conns.TryGetValue(connEntry.Index, out connCtx))
                    {
                        connCtx = new BackPropagationConnectionContext(Rule.CreateRuleContext(connEntry.Connection), connEntry.Connection);
                        Rule.Initialize(connCtx.Connection, connCtx.RuleContext);
                        conns.Add(connEntry.Index, connCtx);
                        if (connEntry.Index.LowerNodeIndex > endIdx)
                        {
                            outputs[connEntry.Index.LowerNodeIndex - endIdx - 1].Add(connCtx);
                        }
                    }
                    lowerCtxs.Add(connCtx);
                }

                contextList.Add(new BackPropagationNetworkEntryContext(nodeCtx, upperCtxs.ToArray(), lowerCtxs.ToArray()));
            }
            contexts = contextList.ToArray();
        }

        protected override void UninitializeData()
        {
            contexts = null;
        }

        protected override NeuralNetworkTestResult Step(NeuralNetworkTest test)
        {
            return onlineTest.RunTest(Network,
                (result) =>
                {
                    BackPropagate(result.GetErrors().First());
                }, 
                false);
        }

        private void BackPropagate(IEnumerable<double> errors)
        {
            int index = 0;
            foreach (double error in errors)
            {
                if (index >= Network.OutputInterface.Length)
                {
                    throw GetInvalidNumberOfErrorsEx();
                }

                SetErrorsToOutputConnections(index, error);

                index++;
            }
            if (index != Network.OutputInterface.Length)
            {
                throw GetInvalidNumberOfErrorsEx();
            }

            BackPropagateErrors();
        }

        #region Errors To Output

        private void SetErrorsToOutputConnections(int index, double error)
        {
            foreach (var connCtx in outputs[index])
            {
                Rule.InitializeOutputConnection(connCtx, error);
            }
        } 

        #endregion

        #region Back Propagate Errors

        private void BackPropagateErrors()
        {
            Rule.BeginBackPropagation(contexts);
            for (int idx = contexts.Length - 1; idx >= 0; idx--)
            {
                Rule.BackPropagationStep(contexts[idx]);
            }
            Rule.EndBackPropagation(contexts);
        }

        #endregion

        #region Error Ex

        private static InvalidOperationException GetInvalidNumberOfErrorsEx()
        {
            return new InvalidOperationException("Invalid number of errors.");
        } 

        #endregion
    }
}
