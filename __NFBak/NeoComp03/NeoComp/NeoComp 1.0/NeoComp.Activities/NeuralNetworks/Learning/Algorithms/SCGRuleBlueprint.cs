using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.NeuralNetworks.Learning.Algorithms;

namespace NeoComp.Activities.NeuralNetworks.Learning.Algorithms
{
    public sealed class SCGRuleBlueprint : LearningRuleBlueprint<SCGRule>
    {
        protected override SCGRule CreateLearningRule(System.Activities.NativeActivityContext context)
        {
            return new SCGRule();
        }

        protected override System.Activities.Activity CreateActivityTemplate(System.Windows.DependencyObject target)
        {
            return new SCGRuleBlueprint { DisplayName = "SCG" };
        }
    }
}
