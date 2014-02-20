using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.NeuralNetworks.Learning;
using System.ComponentModel;
using System.Activities;
using NeoComp.Activities.Internal;
using NeoComp.Activities.Design;
using NeoComp.NeuralNetworks;
using NeoComp.Optimizations;

namespace NeoComp.Activities.NeuralNetworks.Learning
{
    public abstract class TrainingBlueprint : Blueprint<Training>
    {
        [Browsable(false)]
        [ActivityDelegateMetadata(ObjectName = "Neural Network")]
        [BlueprintFunc]
        public ActivityFunc<NeuralNetwork> NeuralNetwork { get; set; }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            if (NeuralNetwork.IsNull()) metadata.AddValidationError("Neural Network is expected.");
            
            base.CacheMetadata(metadata);
        }
    }
}
