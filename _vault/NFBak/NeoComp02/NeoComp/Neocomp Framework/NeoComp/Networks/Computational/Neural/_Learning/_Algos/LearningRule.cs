using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks.Computational.Neural;
using System.Diagnostics.Contracts;

namespace NeoComp.Networks.Computational.Neural
{
    [ContractClass(typeof(LearningRuleContract))]
    public abstract class LearningRule : ILearningRule
    {
        public LearningRule()
        {
            IsEnabled = true;
        }
        
        public bool IsEnabled { get; set; }
        
        public int GroupID { get; set; }

        protected abstract Type AlgorithmType { get; }

        Type ILearningRule.AlgorithmType
        {
            get { return AlgorithmType; }
        }
    }

    [ContractClassFor(typeof(LearningRule))]
    abstract class LearningRuleContract : LearningRule
    {
        protected override Type AlgorithmType
        {
            get
            {
                Contract.Ensures(Contract.Result<Type>() != null);
                Contract.Ensures(Contract.Result<Type>().IsSubclassOf(typeof(LearningAlgorithm)));
                return null;
            }
        }
    }
}
