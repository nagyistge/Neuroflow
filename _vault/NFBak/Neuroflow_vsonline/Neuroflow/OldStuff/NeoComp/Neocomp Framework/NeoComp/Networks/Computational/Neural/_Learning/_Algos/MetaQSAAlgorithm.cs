using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks.Computational.Neural;
using NeoComp.Optimization.Algorithms.Quantum;
using NeoComp.Core;

namespace NeoComp.Networks.Computational.Neural
{
    public class MetaQSADelta : LocalAdaptiveDelta, IQuantumStatedItem
    {
        public QuantumStabilizerAlgorithm QSA { get; internal set; }

        public QuantumState State { get; set; }
    }

    public sealed class MetaQSAAlgorithm : LocalAdaptiveGDAlgorithm<MetaQSARule, MetaQSADelta>
    {
        const double qsaStrength = 1.0;
        const double qsaStabilize = 0.95;
        const double qsaDissolve = 0.001;

        protected override void InitAdaptiveState(IBackwardConnection connection, MetaQSARule rule, MetaQSADelta delta, bool useAverageError)
        {
            base.InitAdaptiveState(connection, rule, delta, useAverageError);
            delta.State = rule.StepSizeRange.Normalize(delta.CurrentStepSize, new DoubleRange(0.0, 1.0));
            delta.QSA = new QuantumStabilizerAlgorithm(delta, qsaStrength);
            delta.CurrentStepSize = ToStepSize(delta.State, rule.StepSizeRange);
        }

        protected override double CalculateCurrentStepSize(IBackwardConnection connection, MetaQSARule rule, MetaQSADelta delta, double signState, bool useAverageError)
        {
            if (signState > 0.0)
            {
                // Stable.
                delta.QSA.Stabilize(qsaStabilize);
            }
            else if (signState < 0.0)
            {
                // Gone wrong.
                delta.QSA.Dissolve(qsaDissolve);
            }
            return ToStepSize(delta.State, rule.StepSizeRange);
        }

        private static double ToStepSize(QuantumState quantumState, DoubleRange ssRange)
        {
            double ss = new DoubleRange(0.0, 1.0).Normalize(quantumState, ssRange);
            return ss;
        }
    }
}
