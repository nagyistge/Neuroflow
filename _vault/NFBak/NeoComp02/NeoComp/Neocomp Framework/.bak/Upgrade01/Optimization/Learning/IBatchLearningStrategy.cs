using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Networks.Neural;

namespace NeoComp.Optimization.Learning
{
    [ContractClass(typeof(IBatchLearningStrategyContract))]
    public interface IBatchLearningStrategy
    {
        void Adjust(double mse, IEnumerable<IEnumerable<double>> errors);
    }

    [ContractClassFor(typeof(IBatchLearningStrategy))]
    class IBatchLearningStrategyContract : IBatchLearningStrategy
    {
        #region IBatchAjusterAlgorithm Members

        void IBatchLearningStrategy.Adjust(double mse, IEnumerable<IEnumerable<double>> errors)
        {
            Contract.Requires(mse >= 0.0 && mse <= 1.0);
            Contract.Requires(errors != null);
        }

        #endregion
    }
}
