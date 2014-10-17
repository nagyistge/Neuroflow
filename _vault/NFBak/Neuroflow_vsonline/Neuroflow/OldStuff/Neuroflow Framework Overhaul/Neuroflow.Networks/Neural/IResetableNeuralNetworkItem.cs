using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neuroflow.Networks.Neural
{
    public interface IResetableNeuralNetworkItem
    {
        IEnumerable<int> GetResetErrorAffectedIndexes();

        IEnumerable<int> GetResetGradientAffectedIndexes();

        IEnumerable<int> GetResetGradientSumAffectedIndexes();
    }
}
