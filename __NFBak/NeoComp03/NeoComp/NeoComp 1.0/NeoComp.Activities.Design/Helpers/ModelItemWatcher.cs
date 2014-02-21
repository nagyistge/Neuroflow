using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Activities.Presentation.Model;
using System.Diagnostics.Contracts;

namespace NeoComp.Activities.Design.Helpers
{
    public abstract class ModelItemWatcher : IWeakEventListener
    {
        protected ModelItemWatcher(ModelItem modelItem)
        {
            Contract.Requires(modelItem != null);

            ModelItem = modelItem;
        }
        
        protected ModelItem ModelItem { get; private set; }

        #region IWeakEventListener Members

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            return ReceiveWeakEvent(managerType, sender, e);
        }

        protected abstract bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e);

        #endregion
    }
}
