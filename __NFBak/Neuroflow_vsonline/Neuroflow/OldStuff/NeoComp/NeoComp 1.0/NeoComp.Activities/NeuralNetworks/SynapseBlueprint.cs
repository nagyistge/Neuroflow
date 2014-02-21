using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.NeuralNetworks;
using System.ComponentModel;
using NeoComp.Activities.Design;
using System.Activities;
using NeoComp.NeuralNetworks.Learning.Algorithms;
using NeoComp.Activities.Internal;

namespace NeoComp.Activities.NeuralNetworks
{
    public sealed class SynapseBlueprint : NeuralComputationConnectionFactoryBlueprint<Synapse>
    {
        [Serializable]
        sealed class SynapseFactory : FactoryBase<Synapse>
        {
            internal LearningRule[] LearningRules { get; set; }

            public override Synapse Create()
            {
                return new Synapse(LearningRules);
            }
        }

        [Browsable(false)]
        [ActivityDelegateMetadata(ObjectName = "Learning Rules")]
        [BlueprintFunc]
        public ActivityFunc<LearningRule[]> LearningRules { get; set; }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            if (LearningRules.IsNull()) metadata.AddValidationError("Learning Rules are expected."); 
            
            base.CacheMetadata(metadata);
        }

        protected override IFactory<Synapse> CreateNeuralComputationConnectionFactory(NativeActivityContext context)
        {
            var rules = GetFuncResult<LearningRule[]>(context, "LearningRules");
            if (rules == null || rules.Length == 0) throw new InvalidOperationException("Learning Rules are expected.");
            return new SynapseFactory { LearningRules = rules };
        }

        protected override Activity CreateActivityTemplate(System.Windows.DependencyObject target)
        {
            return new SynapseBlueprint { DisplayName = "Synapse" };
        }
    }
}
