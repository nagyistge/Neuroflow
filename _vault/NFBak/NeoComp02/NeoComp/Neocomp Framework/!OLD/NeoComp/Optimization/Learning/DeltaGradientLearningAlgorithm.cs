using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks.Computational.Neural;
using System.Diagnostics.Contracts;
using NeoComp.Core;

namespace NeoComp.Optimization.Learning
{
    [ContractClass(typeof(DeltaGradientLearningAlgorithmContract<,>))]
    public abstract class DeltaGradientLearningAlgorithm<TRule, TDelta> : GradientLearningAlgorithm<TRule>
        where TRule : GradientRule
        where TDelta : class, new()
    {
        TDelta[] deltaVector;

        protected ReadOnlyArray<TDelta> DeltaVector { get; private set; }
        
        protected internal override void InitializeNewRun()
        {
            base.InitializeNewRun();

            if (deltaVector == null)
            {
                deltaVector = new TDelta[LearningConnections.Count];
            }
            for (int idx = 0; idx < deltaVector.Length; idx++)
            {
                deltaVector[idx] = new TDelta();
            }
            DeltaVector = ReadOnlyArray.Wrap(deltaVector);
        }

        protected sealed override void StochasticStep(IBackwardConnection connection, TRule rule, int connectionIndex)
        {
            StochasticStep(connection, rule, deltaVector[connectionIndex]);
        }

        protected sealed override void StochasticEOF(IBackwardConnection connection, TRule rule, int connectionIndex)
        {
            StochasticEOF(connection, rule, deltaVector[connectionIndex]);
        }

        protected sealed override void BatchStep(IBackwardConnection connection, TRule rule, int connectionIndex)
        {
            BatchStep(connection, rule, deltaVector[connectionIndex]);
        }

        protected virtual void StochasticStep(IBackwardConnection connection, TRule rule, TDelta delta) { }

        protected virtual void StochasticEOF(IBackwardConnection connection, TRule rule, TDelta delta) { }

        protected virtual void BatchStep(IBackwardConnection connection, TRule rule, TDelta delta) { }
    }

    [ContractClassFor(typeof(DeltaGradientLearningAlgorithm<,>))]
    abstract class DeltaGradientLearningAlgorithmContract<TRule, TDelta> : DeltaGradientLearningAlgorithm<TRule, TDelta>
        where TRule : GradientRule
        where TDelta : GDDelta, new()
    {
        // TODO: Enable this later.
        
        //protected override void StochasticStep(IBackwardConnection connection, T rule, Delta delta)
        //{
        //    Contract.Requires(connection != null);
        //    Contract.Requires(rule != null);
        //    Contract.Requires(delta != null);
        //}

        //protected override void StochasticEOF(IBackwardConnection connection, T rule, Delta delta)
        //{
        //    Contract.Requires(connection != null);
        //    Contract.Requires(rule != null);
        //    Contract.Requires(delta != null);
        //}

        //protected override void BatchStep(IBackwardConnection connection, T rule, Delta delta)
        //{
        //    Contract.Requires(connection != null);
        //    Contract.Requires(rule != null);
        //    Contract.Requires(delta != null);
        //}

        //protected override Delta CreateDelta()
        //{
        //    Contract.Ensures(Contract.Result<Delta>() != null);
        //    return null;
        //}
    }
}
