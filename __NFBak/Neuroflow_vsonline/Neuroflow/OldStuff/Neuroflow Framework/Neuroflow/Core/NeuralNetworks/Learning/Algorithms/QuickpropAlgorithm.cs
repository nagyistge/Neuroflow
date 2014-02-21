using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core.NeuralNetworks;
using System.Diagnostics;

namespace Neuroflow.Core.NeuralNetworks.Learning.Algorithms
{
    public sealed class QuickPropDelta : GDDelta
    {
        public double LastGradient { get; internal set; }
    }

    public sealed class QuickpropAlgorithm : DeltaGradientLearningAlgorithm<QuickpropRule, QuickPropDelta>
    {
        protected override void BatchStep(IBackwardConnection connection, QuickpropRule rule, QuickPropDelta delta)
        {
            double gradient = connection.BackwardValues.AvgGradient;
            double update = GetWeightUpdate(rule, delta, gradient);
            delta.LastGradient = gradient;

            if (rule.WeightDecay != null && rule.WeightDecay.IsEnabled)
            {
                rule.WeightDecay.GetDecayed(update);
            }

            delta.LastUpdate = update;
            connection.Weight = update;
        }

        private static double GetWeightUpdate(QuickpropRule rule, QuickPropDelta delta, double gradient)
        {
            double update = 0.0;

            if (delta.LastUpdate == 0.0)
            {
                update = rule.StepSize * gradient;
            }
            else
            {
                double lastUpdate = delta.LastUpdate;
                double lastGradient = delta.LastGradient;
                double modeSwitchThreshold = rule.ModeSwitchThreshold;

                if (lastUpdate > modeSwitchThreshold)
                {
                    if (gradient > 0)
                    {
                        update = (rule.StepSize * gradient);
                    }

                    if (gradient > (rule.Shrink * lastGradient))
                    {
                        update += (rule.Mu * lastUpdate);
                    }
                    else
                    {
                        update += ((gradient / (lastGradient - gradient)) * lastUpdate);
                    }
                }
                else if (lastUpdate < (-1.0 * modeSwitchThreshold))
                {
                    if (gradient < 0)
                    {
                        update = (rule.StepSize * gradient);
                    }

                    if (gradient < (rule.Shrink * lastGradient))
                    {
                        update += (rule.Mu * lastUpdate);
                    }
                    else
                    {
                        update += ((gradient / (lastGradient - gradient)) * lastUpdate);
                    }
                }
                else
                {
                    update += (rule.StepSize * gradient) + (rule.Momentum * lastUpdate);
                }
            }

            return update;
        }
    }
}
