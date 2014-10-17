using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks.Neural;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.Learning
{
    [ContractClass(typeof(IOnlineLearningStrategyContract))]
    public interface IOnlineLearningStrategy
    {
        void Adjust(double mse, IEnumerable<double> errors, int sampleCount);
    }

    [ContractClassFor(typeof(IOnlineLearningStrategy))]
    class IOnlineLearningStrategyContract : IOnlineLearningStrategy
    {
        #region IOnlineAjusterAlgorithm Members

        void IOnlineLearningStrategy.Adjust(double mse, IEnumerable<double> errors, int sampleCount)
        {
            Contract.Requires(mse >= 0.0 && mse <= 1.0);
            Contract.Requires(errors != null);
            Contract.Requires(sampleCount > 0);
        }

        #endregion
    }
}
