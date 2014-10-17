using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neuroflow.Networks.Neural.Learning.Managed
{
    public sealed class SignChangesAlgorithm : AutoAdjustedGradientDescentAlgorithm<SignChangesRule>
    {
        protected override float CalculateStepSize(float currentStepSize, float lastGradient, float currentGradient)
        {
            if (lastGradient * currentGradient >= 0.0)
            {
                return (float)(currentStepSize * Rule.U);
            }
            else
            {
                return (float)(currentStepSize * Rule.D);
            }
        }
    }
}
