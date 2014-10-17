using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Neuroflow.Design.ActivityDesigners.Interface;
using System.Activities;
using System.Activities.Presentation;
using Neuroflow.Design.ActivityDesigners;

namespace Neuroflow.Activities.NeuralNetworks
{
    [Designer(typeof(TrainingLoopDesigner))]
    public sealed class TrainingLoop : NativeActivityWithVariables, IActivityTemplateFactory
    {
        const string IterationNoVarName = "TrainingLoop.IterationNoVarName";
        
        public Activity<bool> Condition { get; set; }
        
        [Browsable(false)]
        [ActivityDelegateMetadata(ObjectName = "Body", Argument1Name = "Iteration No")]
        public ActivityAction<int> Body { get; set; }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);

            if (Body.IsNull()) metadata.AddValidationError("Body is required.");

            metadata.AddDelegate(Body);

            if (Condition == null) metadata.AddValidationError("Condition is required.");

            metadata.AddChild(Condition);
        }

        protected override void Execute(NativeActivityContext context)
        {
            ExecuteBody(context);
        }

        private void ExecuteBody(NativeActivityContext context)
        {
            var vars = ComputationContext.GetVariables(context, this);

            int no;
            if (!vars.TryGet(IterationNoVarName, out no))
            {
                vars.Set(IterationNoVarName, 1);
            }

            context.ScheduleAction(Body, no, OnExecuteBodyCompleted);
        }

        private void OnExecuteBodyCompleted(NativeActivityContext context, ActivityInstance instance)
        {
            var vars = ComputationContext.GetVariables(context, this);
            vars.Set(IterationNoVarName, vars.Get<int>(IterationNoVarName) + 1);

            context.ScheduleActivity(Condition, OnExecuteConditionCompleted);
        }

        private void OnExecuteConditionCompleted(NativeActivityContext context, ActivityInstance instance, bool result)
        {
            if (result) ExecuteBody(context);
        }

        Activity IActivityTemplateFactory.Create(System.Windows.DependencyObject target)
        {
            return new TrainingLoop
            {
                Body = new ActivityAction<int>
                {
                    Argument = new DelegateInArgument<int>("iterationNo")
                }
            };
        }
    }
}
