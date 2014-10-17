using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core.NeuralNetworks;
using System.Diagnostics.Contracts;
using Neuroflow.ComponentModel;
using System.ComponentModel;

namespace Neuroflow.Core.NeuralNetworks.Learning.Algorithms
{
    [ContractClass(typeof(LearningRuleContract))]
    public abstract class LearningRule : ILearningRule
    {
        public LearningRule()
        {
            IsEnabled = true;
        }

        [InitValue(true)]
        [DefaultValue(true)]
        [Category(PropertyCategories.Behavior)]
        public bool IsEnabled { get; set; }

        [InitValue(0)]
        [DefaultValue(0)]
        [Category(PropertyCategories.Behavior)]
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
