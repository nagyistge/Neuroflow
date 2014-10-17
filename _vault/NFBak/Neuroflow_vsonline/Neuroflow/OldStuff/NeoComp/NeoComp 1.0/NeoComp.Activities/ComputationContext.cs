using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.Collections.ObjectModel;
using System.ComponentModel;
using NeoComp.Activities.Design;
using NeoComp.Collections;
using System.Diagnostics.Contracts;
using System.Activities.Presentation;
using NeoComp.Activities.Internal;
using System.Runtime.Caching;

namespace NeoComp.Activities
{
    [Designer(typeof(ComputationContextDesigner))]
    public class ComputationContext : NativeActivityWithVariables, IActivityTemplateFactory
    {
        const string VariableCollPropName = "ComputationContext.Variables";
        
        public Activity Body { get; set; }

        public static GenericVariableCollection GetVariables(NativeActivityContext context, Activity activity)
        {
            Contract.Requires(context != null);
            Contract.Requires(activity != null);

            var contextVars = (GenericVariableCollection)context.Properties.Find(VariableCollPropName);
            GenericVariableCollection actVars;
            if (contextVars.TryGet(activity.Id, out actVars))
            {
                return actVars;
            }
            else
            {
                actVars = new GenericVariableCollection();
                contextVars.Set(activity.Id, actVars);
                return actVars;
            }
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);

            metadata.AddChild(Body);
        }

        protected override void Execute(NativeActivityContext context)
        {
            if (Body != null)
            {
                context.Properties.Add(VariableCollPropName, new GenericVariableCollection());
                context.ScheduleActivity(Body, OnBodyCompleted);
            }
        }

        private void OnBodyCompleted(NativeActivityContext context, ActivityInstance instance)
        {
            RemoveProperties(context);
        }

        private void OnBodyFaulted(NativeActivityFaultContext faultContext, Exception propagatedException, ActivityInstance propagatedFrom)
        {
            RemoveProperties(faultContext);
        }

        private void RemoveProperties(NativeActivityContext context)
        {
            RemoveVariableCollection(context);
        }

        private void RemoveVariableCollection(NativeActivityContext context)
        {
            var vc = (GenericVariableCollection)context.Properties.Find(VariableCollPropName);
            vc.Dispose();
            context.Properties.Remove(VariableCollPropName);
        }

        #region IActivityTemplateFactory Members

        Activity IActivityTemplateFactory.Create(System.Windows.DependencyObject target)
        {
            return CreateActivityTemplate(target);
        }

        protected virtual ComputationContext CreateActivityTemplate(System.Windows.DependencyObject target)
        {
            return new ComputationContext { DisplayName = "Computation Context" };
        }

        #endregion
    }
}
