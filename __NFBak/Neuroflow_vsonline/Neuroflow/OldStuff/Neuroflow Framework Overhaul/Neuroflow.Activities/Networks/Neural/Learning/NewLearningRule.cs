using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Networks.Neural.Learning;

namespace Neuroflow.Activities.Networks.Neural.Learning
{
    public abstract class NewLearningRule<T> : NewObjectActivity<LearningRule>
        where T : LearningRule
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
