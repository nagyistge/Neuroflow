using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.NeuralNetworks;
using System.ComponentModel;
using System.Activities;
using NeoComp.NeuralNetworks.Learning.Algorithms;

namespace NeoComp.Activities.NeuralNetworks.Learning.Algorithms
{
    public abstract class MultimodeGradientRuleBlueprint<T> : LearningRuleBlueprint<T>
        where T : MultimodeGradientRule
    {
        [Category(PropertyCategories.Behavior)]
        [DefaultExpression("LearningMode.Batch")]
        public virtual InArgument<LearningMode> Mode { get; set; }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            if (Mode == null) metadata.AddValidationError("Learning Mode is expected.");

            base.CacheMetadata(metadata);
        }

        protected sealed override T CreateLearningRule(NativeActivityContext context)
        {
            var rule = CreateMultimodeGradientRule(context);
            rule.Mode = Mode.Get(context);
            return rule;
        }

        protected abstract T CreateMultimodeGradientRule(NativeActivityContext context);
    }
}
