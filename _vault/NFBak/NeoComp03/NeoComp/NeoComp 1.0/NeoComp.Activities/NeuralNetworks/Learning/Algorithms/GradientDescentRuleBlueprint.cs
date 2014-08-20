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
    public class GradientDescentRuleBlueprint : MultimodeGradientRuleBlueprint<GradientDescentRule>
    {
        [Category(PropertyCategories.Math)]
        [DefaultExpression("0.01")]
        public InArgument<double> StepSize { get; set; }

        [Category(PropertyCategories.Math)]
        [DefaultExpression("0.8")]
        public InArgument<double> Momentum { get; set; }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            if (StepSize == null) metadata.AddValidationError("Step Size is expected.");
            if (Momentum == null) metadata.AddValidationError("Momentum is expected.");

            base.CacheMetadata(metadata);
        }

        protected override GradientDescentRule CreateMultimodeGradientRule(NativeActivityContext context)
        {
            var ss = StepSize.Get(context);
            var m = Momentum.Get(context);
            return new GradientDescentRule { StepSize = ss, Momentum = m };
        }

        protected override Activity CreateActivityTemplate(System.Windows.DependencyObject target)
        {
            return new GradientDescentRuleBlueprint { DisplayName = "Gradient Descent Rule" };
        }
    }
}
