using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.SupervisedLearning
{
    public class GDRuleParameters : RuleParameters, IGDRuleParameters
    {
        double stepSize = 0.1;
        
        public double StepSize
        {
            get { return stepSize; }
            set
            {
                Contract.Requires(value >= 0 && value <= 1.0);
                stepSize = value;
            }
        }

        double momentum = 0.1;

        public double Momentum
        {
            get { return momentum; }
            set
            {
                Contract.Requires(value >= 0 && value <= 1.0);
                momentum = value;
            }
        }
    }
}
