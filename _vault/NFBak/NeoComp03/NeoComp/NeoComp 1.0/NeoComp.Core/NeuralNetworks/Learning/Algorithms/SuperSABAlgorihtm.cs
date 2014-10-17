using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.NeuralNetworks;

namespace NeoComp.NeuralNetworks.Learning.Algorithms
{
    public sealed class SuperSABAlgorithm : LocalAdaptiveGDAlgorithm<SuperSABRule>
    {
        protected override double CalculateCurrentStepSize(IBackwardConnection connection, SuperSABRule rule, LocalAdaptiveDelta delta, double signState, bool useAverageError)
        {
            if (signState >= 0.0)
            {
                return delta.CurrentStepSize * rule.U;
            }
            if (signState <= 0.0)
            {
                return delta.CurrentStepSize * rule.D;
            }
            return delta.CurrentStepSize;
        }
    }
}
