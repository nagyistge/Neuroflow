using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities.Presentation.Model;
using System.Diagnostics.Contracts;
using System.Collections.Specialized;
using System.ComponentModel;

namespace NeoComp.Activities.Design.Helpers
{
    public sealed class ModelItemRemoveWatcher : ModelItemWatcher
    {
        public ModelItemRemoveWatcher(ModelItem modelItem)
            : base(modelItem)
        {
            Contract.Requires(modelItem != null);

            parent = modelItem.Parent;
            var parentColl = parent as ModelItemCollection;
            if (parentColl != null)
            {
                CollectionChangedEventManager.AddListener(parentColl, this);
            }
            else if (parent != null)
            {
                parentProp = parent.Properties.Where(pp => pp.Value == modelItem).First();
                PropertyChangedEventManager.AddListener(
                    parent, 
                    this,
                    parentProp.Name);
            }
        }

        ModelItem parent;

        ModelProperty parentProp;

        public event EventHandler Deleted;

        private void OnDeleted()
        {
            var h = Deleted;
            if (h != null) h(this, EventArgs.Empty);
        }

        protected override bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (managerType == typeof(CollectionChangedEventManager))
            {
                ParentCollectionChanged(sender, (NotifyCollectionChangedEventArgs)e);
                return true;
            }
            else if (managerType == typeof(PropertyChangedEventManager))
            {
                ParentPropertyChanged(sender, (PropertyChangedEventArgs)e);
                return true;
            }
            return false;
        }

        private void ParentPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (parentProp.Value != ModelItem)
            {
                PropertyChangedEventManager.RemoveListener(parent, this, parentProp.Name);
                OnDeleted();
            }
        }

        private void ParentCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems.Contains(ModelItem))
            {
                CollectionChangedEventManager.RemoveListener((ModelItemCollection)parent, this);
                OnDeleted();
            }
        }
    }
}
