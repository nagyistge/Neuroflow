using System;using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks.Computational.Neural;

namespace NeoComp.Networks.Computational.Neural
{
    public sealed class RpropDelta : LocalAdaptiveDelta
    {
        internal double RPUpdate { get; set; }
    }

    public sealed class RpropAlgorithm : LocalAdaptiveGDAlgorithm<RpropRule, RpropDelta>
    {
        protected override void StochasticStep(IBackwardConnection connection, RpropRule rule, RpropDelta delta)
        {
            UpdateWeight(connection, rule, delta, GetStepSize(rule, delta));
        }

        protected override void BatchStep(IBackwardConnection connection, RpropRule rule, RpropDelta delta)
        {
            UpdateWeight(connection, rule, delta, GetStepSize(rule, delta));
        }
        
        protected override void InitAdaptiveState(IBackwardConnection connection, RpropRule rule, RpropDelta delta, bool useAverageError)
        {
            base.InitAdaptiveState(connection, rule, delta, useAverageError);
            delta.RPUpdate = rule.StepSize;
        }

        protected override double CalculateCurrentStepSize(IBackwardConnection connection, RpropRule rule, RpropDelta delta, double signState, bool useAverageError)
        {
            double stepSize = 0.0;
            double error = delta.CurrentError;

            if (signState >= 0)
            { 
                // no sign change

                if (signState > 0)
                {
                    delta.RPUpdate *= rule.U;
                    if (delta.RPUpdate > rule.StepSizeRange.MaxValue)
                    {
                        delta.RPUpdate = rule.StepSizeRange.MaxValue;
                    }
                }

                // modify the weight
                if (error > 0)
                {
                    stepSize = delta.RPUpdate;
                }
                else //if (error < 0)
                {
                    stepSize = -delta.RPUpdate;
                }
            }
            else if (signState < 0)
            {
                // sign change
                delta.RPUpdate *= rule.D;
                if (delta.RPUpdate < rule.StepSizeRange.MinValue)
                {
                    delta.RPUpdate = rule.StepSizeRange.MinValue;
                }
                delta.CurrentError = 0;
                stepSize = 0;
            }

            return stepSize;
        }
    }
}
