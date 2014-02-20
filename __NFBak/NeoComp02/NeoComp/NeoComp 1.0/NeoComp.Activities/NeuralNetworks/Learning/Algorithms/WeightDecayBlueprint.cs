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
    public class WeightDecayBlueprint : Blueprint<WeightDecay>
    {
        [Category(PropertyCategories.Math)]
        [DefaultExpression("2.0")]
        public InArgument<double> Cutoff { get; set; }

        [Category(PropertyCategories.Math)]
        [DefaultExpression("-0.00001")]
        public InArgument<double> Factor { get; set; }

        [Category(PropertyCategories.Behavior)]
        [DefaultExpression("True")]
        public InArgument<bool> IsEnabled { get; set; }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            if (Cutoff == null) metadata.AddValidationError("Cutoff is expected.");
            if (Factor == null) metadata.AddValidationError("Factor is expected.");

            base.CacheMetadata(metadata);
        }

        protected override WeightDecay CreateObject(NativeActivityContext context)
        {
            double cutoff = Cutoff.Get(context);
            double factor = Factor.Get(context);
            bool isEnabled = IsEnabled != null ? IsEnabled.Get(context) : true;
            return new WeightDecay { Cutoff = cutoff, Factor = factor, IsEnabled = isEnabled };
        }

        protected override Activity CreateActivityTemplate(System.Windows.DependencyObject target)
        {
            return new WeightDecayBlueprint { DisplayName = "Weight Decay" };
        }
    }
}
