using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Neuroflow.Core.NeuralNetworks.Learning
{
    [ContractClass(typeof(ILearningRuleContract))]
    public interface ILearningRule
    {
        bool IsEnabled { get; }
        
        int GroupID { get; }

        Type AlgorithmType { get; }
    }

    [ContractClassFor(typeof(ILearningRule))]
    class ILearningRuleContract : ILearningRule
    {
        int ILearningRule.GroupID
        {
            get
            {
                return 0;
            }
        }

        Type ILearningRule.AlgorithmType
        {
            get
            {
                Contract.Ensures(Contract.Result<Type>() != null);
                Contract.Ensures(Contract.Result<Type>().IsSubclassOf(typeof(LearningAlgorithm)));
                return null;
            }
        }

        bool ILearningRule.IsEnabled
        {
            get { return false; }
        }
    }
}
