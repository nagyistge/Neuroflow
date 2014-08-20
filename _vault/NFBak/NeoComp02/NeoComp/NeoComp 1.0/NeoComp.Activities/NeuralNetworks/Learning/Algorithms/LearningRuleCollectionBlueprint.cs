using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.NeuralNetworks;
using System.Activities;
using NeoComp.NeuralNetworks.Learning.Algorithms;

namespace NeoComp.Activities.NeuralNetworks.Learning.Algorithms
{
    public sealed class LearningRuleCollectionBlueprint : CollectionBlueprint<LearningRule>
    {
        protected override Activity CreateCollectionBlueprintTemplate(System.Windows.DependencyObject target)
        {
            return new LearningRuleCollectionBlueprint { DisplayName = "Learning Rules" };
        }
    }
}
