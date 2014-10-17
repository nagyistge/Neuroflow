using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.NeuralNetworks;
using NeoComp;
using System.ComponentModel;
using NeoComp.Activities.Design;
using System.Activities;
using NeoComp.Activities.Internal;
using NeoComp.NeuralNetworks.ActivationFunctions;
using NeoComp.NeuralNetworks.Learning.Algorithms;

namespace NeoComp.Activities.NeuralNetworks
{
    public class ActivationNeuronFactoryBlueprint : NeuralOperationNodeFactoryBlueprint<ActivationNeuron>
    {
        [Serializable]
        sealed class ActivationNeuronFactory : FactoryBase<ActivationNeuron>
        {
            internal IActivationFunction ActivationFunction { get; set; }

            internal LearningRule[] LearningRules { get; set; }

            public override ActivationNeuron Create()
            {
                return new ActivationNeuron(ActivationFunction, LearningRules);
            }
        }
        
        [Browsable(false)]
        [ActivityDelegateMetadata(ObjectName = "Activation Function")]
        [BlueprintFunc]
        public ActivityFunc<IActivationFunction> ActivationFunction { get; set; }

        [Browsable(false)]
        [ActivityDelegateMetadata(ObjectName = "Learning Rules")]
        [BlueprintFunc]
        public ActivityFunc<LearningRule[]> LearningRules { get; set; }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            if (ActivationFunction.IsNull()) metadata.AddValidationError("Activation Function is expected.");
            if (LearningRules.IsNull()) metadata.AddValidationError("Learning Rules are expected.");
            
            base.CacheMetadata(metadata);
        }

        protected override IFactory<ActivationNeuron> CreateNeuralOperationNodeFactory(NativeActivityContext context)
        {
            var af = GetFuncResult<IActivationFunction>(context, "ActivationFunction");
            var rules = GetFuncResult<LearningRule[]>(context, "LearningRules");
            if (af == null) throw new InvalidOperationException("Activation Function is expected.");
            if (rules == null || rules.Length == 0) throw new InvalidOperationException("Learning Rules are expected.");
            return new ActivationNeuronFactory { ActivationFunction = af, LearningRules = rules };
        }

        protected override Activity CreateActivityTemplate(System.Windows.DependencyObject target)
        {
            return new ActivationNeuronFactoryBlueprint { DisplayName = "Activation Neuron" };
        }
    }
}
