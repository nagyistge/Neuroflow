using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.NeuralNetworks.Learning.Algorithms;
using System.ComponentModel;
using System.Activities;

namespace NeoComp.Activities.NeuralNetworks.Learning.Algorithms
{
    public sealed class NoisedWeightInitializationRuleBlueprint : LearningRuleBlueprint<NoisedWeightInitializationRule>
    {
        [Category(PropertyCategories.Math)]
        [DefaultExpression("0.3")]
        public InArgument<double> Noise { get; set; }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            if (Noise == null) metadata.AddValidationError("Noise is expected.");

            base.CacheMetadata(metadata);
        }

        protected override NoisedWeightInitializationRule CreateLearningRule(NativeActivityContext context)
        {
            return new NoisedWeightInitializationRule { Noise = Noise.Get(context) };
        }

        protected override Activity CreateActivityTemplate(System.Windows.DependencyObject target)
        {
            return new NoisedWeightInitializationRuleBlueprint { DisplayName = "Noised Weight Init." };
        }
    }
}
