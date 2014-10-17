using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities.Core.Presentation;
using System.Activities.Presentation;
using System.Activities.Presentation.Metadata;
using System.Activities.Presentation.Toolbox;
using System.Activities.Statements;
using System.ComponentModel;
using System.Windows;
using NeoComp.Activities;
using NeoComp.NeuralNetworks;
using NeoComp.Activities.NeuralNetworks.Architectures;
using NeoComp.Activities.NeuralNetworks.ActivationFunctions;
using NeoComp.Activities.NeuralNetworks;
using NeoComp.Activities.NeuralNetworks.Learning.Algorithms;
using NeoComp.Activities.NeuralNetworks.Learning;

namespace WFTestHost
{
    /// <summary>
    /// Interaction logic for RehostingWFDesigner.xaml
    /// </summary>
    public partial class RehostingWFDesigner : Window
    {
        public RehostingWFDesigner()
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            // register metadata
            (new DesignerMetadata()).Register();
            RegisterCustomMetadata();

            // add custom activity to toolbox
            Toolbox.Categories.Add(new ToolboxCategory("NeoComp Activities"));
            Toolbox.Categories[1].Add(new ToolboxItemWrapper(typeof(ComputationContext)));
            Toolbox.Categories[1].Add(new ToolboxItemWrapper(typeof(WiredNeuralNetworkBlueprint)));
            Toolbox.Categories[1].Add(new ToolboxItemWrapper(typeof(WeightDecayBlueprint)));
            Toolbox.Categories[1].Add(new ToolboxItemWrapper(typeof(GradientDescentRuleBlueprint)));
            Toolbox.Categories[1].Add(new ToolboxItemWrapper(typeof(LearningRuleCollectionBlueprint)));
            Toolbox.Categories[1].Add(new ToolboxItemWrapper(typeof(LinearActivationFunctionBlueprint)));
            Toolbox.Categories[1].Add(new ToolboxItemWrapper(typeof(SigmoidActivationFunctionBlueprint)));
            Toolbox.Categories[1].Add(new ToolboxItemWrapper(typeof(ActivationNeuronFactoryBlueprint)));
            Toolbox.Categories[1].Add(new ToolboxItemWrapper(typeof(UnorderedBatcher)));

            // create the workflow designer
            WorkflowDesigner wd = new WorkflowDesigner();
            wd.Load(new Sequence());
            DesignerBorder.Child = wd.View;
            PropertyBorder.Child = wd.PropertyInspectorView;

        }

        private void RegisterCustomMetadata()
        {
        }
    }
}
