using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Networks.Neural;

namespace NeoComp.Optimization.Learning
{
    [ContractClass(typeof(IParallelLearningStrategyContract))]
    public interface IParallelLearningStrategy
    {
        NeuralNetworkTestResult DoIteration(NeuralNetworkTest test);
    }

    [ContractClassFor(typeof(IParallelLearningStrategy))]
    class IParallelLearningStrategyContract : IParallelLearningStrategy
    {
        NeuralNetworkTestResult IParallelLearningStrategy.DoIteration(NeuralNetworkTest test)
        {
            Contract.Requires(test != null);
            Contract.Ensures(Contract.Result<NeuralNetworkTestResult>() != null);
            return null;
        }
    }
}
