using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities.Presentation;
using System.Windows;
using System.Diagnostics.Contracts;
using System.Windows.Media;
using System.Activities.Presentation.View;

namespace Neuroflow.Design.ActivityDesigners.ViewState
{
    public static class ViewStateHelperExtensions
    {
        public static WorkflowViewElement FindWorkflowViewElement(this DependencyObject obj)
        {
            Contract.Requires(obj != null);

            var current = obj;
            while (current != null)
            {
                var wve = current as WorkflowViewElement;

                if (wve != null) return wve;

                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }
        
        public static EditingContext FindEditingContext(this DependencyObject obj)
        {
            Contract.Requires(obj != null);

            var element = obj.FindWorkflowViewElement();

            if (element != null) return element.Context;

            return null;
        }

        public static ViewStateService FindViewStateService(this DependencyObject obj)
        {
            Contract.Requires(obj != null);

            var ctx = obj.FindEditingContext();

            if (ctx != null) return ctx.Services.GetService<ViewStateService>();

            return null;
        }
    }
}
