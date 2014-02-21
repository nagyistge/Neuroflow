using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities.Presentation.Model;
using System.Diagnostics.Contracts;
using System.Activities;

namespace NeoComp.Activities.Design.ViewModel
{
    public sealed class ActivityDelegateItems
    {
        public ActivityDelegateItems(ModelItem modelItem)
        {
            Contract.Requires(modelItem != null);
            ModelItem = modelItem;
            Items = Find();
        }

        public ModelItem ModelItem { get; private set; }

        public ActivityDelegateItem[] Items { get; private set; }

        private ActivityDelegateItem[] Find()
        {
            return (from prop in ModelItem.Properties
                    let propType = prop.PropertyType
                    where propType.IsSubclassOf(typeof(ActivityDelegate))
                    let mi = prop.Value
                    where mi != null
                    let attrib = prop.Attributes.OfType<ActivityDelegateMetadataAttribute>().FirstOrDefault()
                    let item = new ActivityDelegateItem(mi, attrib)
                    orderby attrib.Order, item.ObjectName
                    select item).ToArray();
        }
    }
}
