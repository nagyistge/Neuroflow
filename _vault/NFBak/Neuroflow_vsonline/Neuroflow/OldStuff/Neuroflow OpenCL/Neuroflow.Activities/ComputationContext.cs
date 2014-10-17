using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Activities.Presentation;
using System.Runtime.Caching;

namespace Neuroflow.Activities
{
    [Designer(Designers.ComputationContextDesigner)]
    public class ComputationContext : NativeActivityWithVariables
    {
        const string VariableCollPropName = "ComputationContext.Variables";
        
        [Browsable(false)]
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
    }
}
