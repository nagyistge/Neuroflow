using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core.NeuralNetworks;
using System.Diagnostics.Contracts;

namespace Neuroflow.Core.NeuralNetworks.Learning.Algorithms
{
    [ContractClass(typeof(GradientLearningAlgorithmContract<>))]
    public abstract class GradientLearningAlgorithm<TRule> : BackwardLearningAlgorithm
        where TRule : GradientRule
    {
        struct Info
        {
            internal TRule rule;
            internal IBackwardConnection connection;
            internal LearningMode mode;
        }

        Info[] infos;

        protected sealed override bool WantBackpropagation
        {
            get { return true; }
        }

        protected internal override void InitializeNewRun()
        {
            base.InitializeNewRun();

            infos = new Info[LearningConnections.Count];
            int idx = 0;
            foreach (var lc in LearningConnections.ItemArray)
            {
                var rule = (TRule)lc.Rule;
                var conn = (IBackwardConnection)lc.Connection;
                var lmode = rule.GetMode();
                infos[idx++] = new Info { connection = conn, mode = lmode, rule = rule };
            }
        }
        
        protected internal override void BackwardIteration(bool batch, double mse)
        {
            base.BackwardIteration(batch, mse);

            for (int idx = 0; idx < infos.Length; idx++)
            {
                var info = infos[idx];

                if (info.mode == LearningMode.Stochastic)
                {
                    StochasticStep(info.connection, info.rule, idx);
                }
                
                if (batch)
                {
                    if (info.mode == LearningMode.Batch)
                    {
                        BatchStep(info.connection, info.rule, idx);
                    }
                    else
                    {
                        StochasticEOF(info.connection, info.rule, idx);
                    }
                }
            }
        }

        protected virtual void StochasticStep(IBackwardConnection connection, TRule rule, int connectionIndex) { }

        protected virtual void StochasticEOF(IBackwardConnection connection, TRule rule, int connectionIndex) { }

        protected virtual void BatchStep(IBackwardConnection connection, TRule rule, int connectionIndex) { }
    }

    [ContractClassFor(typeof(GradientLearningAlgorithm<>))]
    abstract class GradientLearningAlgorithmContract<T> : GradientLearningAlgorithm<T>
        where T : GradientRule
    {
        protected override void BatchStep(IBackwardConnection connection, T rule, int connectionIndex)
        {
            Contract.Requires(connection != null);
            Contract.Requires(rule != null);
            Contract.Requires(connectionIndex >= 0 && connectionIndex < LearningConnections.Count);
        }

        protected override void StochasticStep(IBackwardConnection connection, T rule, int connectionIndex)
        {
            Contract.Requires(connection != null);
            Contract.Requires(rule != null);
            Contract.Requires(connectionIndex >= 0 && connectionIndex < LearningConnections.Count);
        }

        protected override void StochasticEOF(IBackwardConnection connection, T rule, int connectionIndex)
        {
            Contract.Requires(connection != null);
            Contract.Requires(rule != null);
            Contract.Requires(connectionIndex >= 0 && connectionIndex < LearningConnections.Count);
        }
    }
}
