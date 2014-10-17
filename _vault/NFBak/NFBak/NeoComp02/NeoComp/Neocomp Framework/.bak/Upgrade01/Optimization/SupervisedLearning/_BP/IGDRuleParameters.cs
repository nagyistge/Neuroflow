using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.SupervisedLearning
{
    [ContractClass(typeof(IGDRuleParametersContract))]
    public interface IGDRuleParameters : IRuleParameters
    {
        double StepSize { get; }

        double Momentum { get; }
    }

    [ContractClassFor(typeof(IGDRuleParameters))]
    class IGDRuleParametersContract : IGDRuleParameters
    {
        double IGDRuleParameters.StepSize
        {
            get
            {
                Contract.Ensures(Contract.Result<double>() >= 0.0 && Contract.Result<double>() <= 1.0);
                return 0;
            }
        }

        double IGDRuleParameters.Momentum
        {
            get
            {
                Contract.Ensures(Contract.Result<double>() >= 0.0 && Contract.Result<double>() <= 1.0);
                return 0;
            }
        }

        double IRuleParameters.WeightInititlizationNoise
        {
            get { return 0; }
        }
    }
}
