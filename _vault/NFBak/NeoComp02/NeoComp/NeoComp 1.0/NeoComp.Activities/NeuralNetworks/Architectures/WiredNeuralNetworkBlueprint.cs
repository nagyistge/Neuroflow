using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.NeuralNetworks.Architectures;
using System.ComponentModel;
using System.Activities;
using NeoComp.Activities.Design;
using NeoComp.Activities.Internal;
using NeoComp;
using NeoComp.ComputationalNetworks;

namespace NeoComp.Activities.NeuralNetworks.Architectures
{
    public sealed class WiredNeuralNetworkBlueprint : NeuralNetworkBlueprint<WiredNeuralArchitecture>
    {
        [Category(PropertyCategories.Architecture)]
        public InArgument<int> NodeCount { get; set; }

        [Category(PropertyCategories.Architecture)]
        [DefaultExpression("False")]
        public InArgument<bool> IsRecurrent { get; set; }

        [Browsable(false)]
        [ActivityDelegateMetadata(ObjectName = "Node Factory", Order = 1)]
        [BlueprintFunc]
        public ActivityFunc<IFactory<OperationNode<double>>> NodeFactory { get; set; }

        [Browsable(false)]
        [ActivityDelegateMetadata(ObjectName = "Collector Node Factory", Order = 2)]
        [BlueprintFunc]
        public ActivityFunc<IFactory<OperationNode<double>>> CollectorNodeFactory { get; set; }

        [Browsable(false)]
        [ActivityDelegateMetadata(ObjectName = "Connection Factory", Order = 0)]
        [BlueprintFunc]
        public ActivityFunc<IFactory<ComputationConnection<double>>> ConnectionFactory { get; set; }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            if (NodeCount == null) metadata.AddValidationError("Node Count is expected.");
            if (NodeFactory.IsNull()) metadata.AddValidationError("Node Factory is expected.");
            if (CollectorNodeFactory.IsNull()) metadata.AddValidationError("Collector Node Factory is expected.");
            if (ConnectionFactory.IsNull()) metadata.AddValidationError("Connection Factory is expected.");
            
            base.CacheMetadata(metadata);
        }

        protected override WiredNeuralArchitecture CreateNeuralArchitecture(NativeActivityContext context, int inputInterfaceLength, int outputInterfaceLength)
        {
            bool isRecurrent = false;
            if (IsRecurrent != null) isRecurrent = IsRecurrent.Get(context);
            int nodeCount = NodeCount.Get(context);
            var nodeFactory = GetFuncResult<IFactory<OperationNode<double>>>(context, "NodeFactory");
            var collNodeFactory = GetFuncResult<IFactory<OperationNode<double>>>(context, "CollectorNodeFactory");
            var connectionFactory = GetFuncResult<IFactory<ComputationConnection<double>>>(context, "ConnectionFactory");

            return new WiredNeuralArchitecture(
                inputInterfaceLength, 
                outputInterfaceLength, 
                nodeCount, 
                nodeFactory, 
                collNodeFactory, 
                connectionFactory, 
                isRecurrent);
        }

        protected override Activity CreateActivityTemplate(System.Windows.DependencyObject target)
        {
            return new WiredNeuralNetworkBlueprint { DisplayName = "Wired Neural Network" };
        }
    }
}
