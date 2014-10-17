using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.NeuralNetworks;
using System.Diagnostics.Contracts;

namespace NeoComp.NeuralNetworks.Learning.Algorithms
{
    public class LocalAdaptiveDelta : GDDelta
    {
        public bool HasAdaptiveState { get; internal set; }
        
        public double LastError { get; internal set; }

        public double CurrentError { get; internal set; }

        public double CurrentStepSize { get; internal set; }
    }

    public abstract class LocalAdaptiveGDAlgorithm<TRule> : LocalAdaptiveGDAlgorithm<TRule, LocalAdaptiveDelta>
        where TRule : LocalAdaptiveGDRule
    {
    }

    [ContractClass(typeof(LocalAdaptiveGDAlgorithmContract<,>))]
    public abstract class LocalAdaptiveGDAlgorithm<TRule, TDelta> : GradientDescentAlgorithm<TRule, TDelta>
        where TRule : LocalAdaptiveGDRule
        where TDelta : LocalAdaptiveDelta, new()
    {
        protected override void StochasticStep(IBackwardConnection connection, TRule rule, TDelta delta)
        {
            if (rule.StochasticAdaptiveStateUpdate) StepAdaptiveState(connection, rule, delta, false);
            base.StochasticStep(connection, rule, delta);
        }
        
        protected override void StochasticEOF(IBackwardConnection connection, TRule rule, TDelta delta)
        {
            if (!rule.StochasticAdaptiveStateUpdate) StepAdaptiveState(connection, rule, delta, true);
        }

        protected override void BatchStep(IBackwardConnection connection, TRule rule, TDelta delta)
        {
            StepAdaptiveState(connection, rule, delta, true);
            base.BatchStep(connection, rule, delta);
        }

        private void StepAdaptiveState(IBackwardConnection connection, TRule rule, TDelta delta, bool useAverageError)
        {
            if (!delta.HasAdaptiveState)
            {
                InitAdaptiveState(connection, rule, delta, useAverageError);
                delta.HasAdaptiveState = true;
            }
            else
            {
                UpdateAdaptiveState(connection, rule, delta, useAverageError);
                double signState = delta.CurrentError * delta.LastError;
                delta.CurrentStepSize = rule.StepSizeRange.Cut(CalculateCurrentStepSize(connection, rule, delta, signState, useAverageError));
            }
        }

        protected virtual void UpdateAdaptiveState(IBackwardConnection connection, TRule rule, TDelta delta, bool useAverageError)
        {
            double error = useAverageError ? connection.BackwardValues.AvgGradient : connection.BackwardValues.Last.Gradient;
            delta.LastError = delta.CurrentError;
            delta.CurrentError = error;
        }

        protected virtual void InitAdaptiveState(IBackwardConnection connection, TRule rule, TDelta delta, bool useAverageError)
        {
            double error = useAverageError ? connection.BackwardValues.AvgGradient : connection.BackwardValues.Last.Gradient;
            delta.LastError = delta.CurrentError = error;
            delta.CurrentStepSize = rule.StepSize;
        }

        protected abstract double CalculateCurrentStepSize(IBackwardConnection connection, TRule rule, TDelta delta, double signState, bool useAverageError);

        protected override double GetStepSize(TRule rule, TDelta delta)
        {
            return delta.HasAdaptiveState ? delta.CurrentStepSize : rule.StepSize;
        }
    }

    [ContractClassFor(typeof(LocalAdaptiveGDAlgorithm<,>))]
    abstract class LocalAdaptiveGDAlgorithmContract<TRule, TDelta> : LocalAdaptiveGDAlgorithm<TRule, TDelta>
        where TRule : LocalAdaptiveGDRule
        where TDelta : LocalAdaptiveDelta, new()
    {
        // TRuleODO: Enable this later.
        
        //protected override double CalculateCurrentStepSize(bool isStochastic, IBackwardConnection connection, TRule rule, LocalAdaptiveTDelta delta, double signState)
        //{
        //    Contract.Requires(connection != null);
        //    Contract.Requires(rule != null);
        //    Contract.Requires(delta != null);
        //    return 0.0;
        //}

        //protected override void UpdateAdaptiveState(bool isStochastic, IBackwardConnection connection, TRule rule, LocalAdaptiveTDelta delta)
        //{
        //    Contract.Requires(connection != null);
        //    Contract.Requires(rule != null);
        //    Contract.Requires(delta != null);
        //}

        //protected override void InitAdaptiveState(bool isStochastic, IBackwardConnection connection, TRule rule, LocalAdaptiveTDelta delta)
        //{
        //    Contract.Requires(connection != null);
        //    Contract.Requires(rule != null);
        //    Contract.Requires(delta != null);
        //}
    }
}
