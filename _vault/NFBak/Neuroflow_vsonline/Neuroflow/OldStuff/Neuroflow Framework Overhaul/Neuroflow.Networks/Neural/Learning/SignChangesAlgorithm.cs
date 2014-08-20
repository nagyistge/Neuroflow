using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core.ComputationAPI;

namespace Neuroflow.Networks.Neural.Learning
{
    public sealed class SignChangesAlgorithm : AutoAdjustedGradientDescentAlgorithm<SignChangesRule>
    {
        protected override unsafe double CalculateStepSize(double currentStepSize, double lastGradient, double currentGradient)
        {
            if (lastGradient * currentGradient >= 0.0)
            {
                return currentStepSize * Rule.U;
            }
            else
            {
                return currentStepSize * Rule.D;
            }
        }
    }
}
