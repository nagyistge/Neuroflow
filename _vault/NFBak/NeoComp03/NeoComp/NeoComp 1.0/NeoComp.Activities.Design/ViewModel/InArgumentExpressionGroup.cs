using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities.Presentation.Model;
using System.Diagnostics.Contracts;
using System.ComponentModel;

namespace NeoComp.Activities.Design.ViewModel
{
    public sealed class InArgumentExpressionGroup
    {
        public InArgumentExpressionGroup(ModelItem ownerModelItem, string groupName)
        {
            Contract.Requires(ownerModelItem != null);
            Contract.Requires(!string.IsNullOrEmpty(groupName));

            OwnerModelItem = ownerModelItem;
            GroupName = groupName;
            Expressions = Find();
        }

        public ModelItem OwnerModelItem { get; private set; }

        public string GroupName { get; private set; }

        public InArgumentExpression[] Expressions { get; private set; }

        private InArgumentExpression[] Find()
        {
            return (from prop in OwnerModelItem.Properties
                    let mi = prop.Value
                    where mi != null
                    let cat = prop.Attributes.OfType<CategoryAttribute>().Select(a => a.Category).FirstOrDefault()
                    where cat == GroupName
                    let iae = new InArgumentExpression(prop.Name, OwnerModelItem, mi)
                    let order = prop.Attributes.OfType<OrderAttribute>().Select(a => a.Order).FirstOrDefault()
                    orderby order, iae.Name
                    select iae).ToArray();
        }
    }
}
