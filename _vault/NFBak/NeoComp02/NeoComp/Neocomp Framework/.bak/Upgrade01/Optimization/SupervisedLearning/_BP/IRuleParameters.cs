using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.SupervisedLearning
{
    [ContractClass(typeof(IRuleParametersContract))]
    public interface IRuleParameters
    {
        double WeightInititlizationNoise { get; }
    }

    [ContractClassFor(typeof(IRuleParameters))]
    class IRuleParametersContract : IRuleParameters
    {
        double IRuleParameters.WeightInititlizationNoise
        {
            get
            {
                Contract.Ensures(Contract.Result<double>() >= 0.0 && Contract.Result<double>() <= 1.0);
                return 0;
            }
        }
    }
}
