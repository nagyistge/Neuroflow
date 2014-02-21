using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks.Computational.Neural;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.Learning
{
    public class GDDelta
    {
        public double LastUpdate { get; internal set; }
    }
    
    public sealed class GradientDescentAlgorithm : GradientDescentAlgorithm<GradientDescentRule, GDDelta> { }
    
    public class GradientDescentAlgorithm<TRule, TDelta> : DeltaGradientLearningAlgorithm<TRule, TDelta>
        where TRule : GradientDescentRule
        where TDelta : GDDelta, new()
    {
        protected override void StochasticStep(IBackwardConnection connection, TRule rule, TDelta delta)
        {
            UpdateWeight(connection, rule, delta, connection.BackwardValues.Last.Gradient * GetStepSize(rule, delta));
        }

        protected override void BatchStep(IBackwardConnection connection, TRule rule, TDelta delta)
        {
            UpdateWeight(connection, rule, delta, connection.BackwardValues.AvgGradient * GetStepSize(rule, delta));
        }

        protected static void UpdateWeight(IBackwardConnection connection, TRule rule, TDelta delta, double update)
        {
            connection.Weight += (delta.LastUpdate = ((rule.Momentum * delta.LastUpdate) + update));
        }

        protected virtual double GetStepSize(TRule rule, TDelta delta)
        {
            Contract.Requires(rule != null);
            Contract.Requires(delta != null);

            return rule.StepSize;
        }
    }
}
