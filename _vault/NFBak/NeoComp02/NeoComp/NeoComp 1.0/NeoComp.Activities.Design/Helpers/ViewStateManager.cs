using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Activities.Presentation.View;
using System.Activities.Presentation.Model;
using NeoComp.WPF.Helpers;
using System.Activities.Presentation;
using System.Diagnostics.Contracts;
using System.Windows.Threading;

namespace NeoComp.Activities.Design.Helpers
{
    public class ViewStateManager
    {
        public ViewStateManager(DependencyObject owner)
        {
            Contract.Requires(owner != null);
            
            Owner = owner;
            Dispatcher.CurrentDispatcher.BeginInvoke(new Action(Initialize));
        }

        public DependencyObject Owner { get; private set; }

        public ActivityDesigner Designer { get; private set; }

        public ViewStateService VSS { get; private set; }

        private void Initialize()
        {
            Designer = WPFTree.TryFindParent<ActivityDesigner>(Owner);
            if (Designer != null)
            {
                VSS = Designer.Context.Services.GetService<ViewStateService>();
            }
        }

        public void Save(string key, object value)
        {
            if (Designer != null)
            {
                VSS.StoreViewState(Designer.ModelItem, key, value);
            }
        }

        public bool Get(string key, out object value)
        {
            if (Designer != null)
            {
                object v = VSS.RetrieveViewState(Designer.ModelItem, key);
                if (v != null)
                {
                    value = v;
                    return true;
                }
            }
            value = null;
            return false;
        }
    }
}
