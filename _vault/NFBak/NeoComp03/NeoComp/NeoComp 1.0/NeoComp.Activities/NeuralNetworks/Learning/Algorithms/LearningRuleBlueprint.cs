using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.NeuralNetworks;
using NeoComp.Internal;
using System.ComponentModel;
using System.Activities;
using NeoComp.Activities.Design;
using NeoComp.NeuralNetworks.Learning.Algorithms;

namespace NeoComp.Activities.NeuralNetworks.Learning.Algorithms
{
    public abstract class LearningRuleBlueprint<T> : Blueprint<LearningRule>
        where T : LearningRule
    {
        static readonly bool isWeightDecayed = typeof(T).Implements<IWeightDecayedLearningRule>();

        public bool IsWeightDecayed
        {
            get { return isWeightDecayed; }
        }

        [Category(PropertyCategories.Behavior)]
        [DefaultExpression("True")]
        public InArgument<bool> IsEnabled { get; set; }

        [Category(PropertyCategories.Behavior)]
        public InArgument<int> GroupID { get; set; }

        [Browsable(false)]
        [ActivityDelegateMetadata(ObjectName = "Weight Decay")]
        [BlueprintFunc]
        public ActivityFunc<WeightDecay> WeightDecay { get; set; }

        protected sealed override LearningRule CreateObject(NativeActivityContext context)
        {
            var rule = CreateLearningRule(context);
            var iwd = rule as IWeightDecayedLearningRule;
            if (iwd != null)
            {
                var wd = GetFuncResult<WeightDecay>(context, "WeightDecay");
                iwd.WeightDecay = wd;
            }
            return rule;
        }

        protected abstract T CreateLearningRule(NativeActivityContext context);
    }
}
