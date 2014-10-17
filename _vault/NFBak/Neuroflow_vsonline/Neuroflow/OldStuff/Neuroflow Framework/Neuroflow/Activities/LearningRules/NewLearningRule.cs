using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core.NeuralNetworks.Learning;

namespace Neuroflow.Activities.LearningRules
{
    public abstract class NewLearningRule<T> : NewObjectActivity<ILearningRule>
        where T : class, ILearningRule
    {
        public NewLearningRule()
            : base()
        {
            if (DisplayName.EndsWith("Rule")) DisplayName = DisplayName.Substring(0, DisplayName.Length - "Rule".Length);
        }
        
        protected override Type ObjectType
        {
            get { return typeof(T); }
        }
    }
}
