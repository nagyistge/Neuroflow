using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities.Presentation.Model;
using System.Diagnostics.Contracts;
using System.ComponentModel;

namespace NeoComp.Activities.Design.ViewModel
{
    public sealed class InArgumentGroups
    {
        public InArgumentGroups(ModelItem modelItem)
        {
            Contract.Requires(modelItem != null);

            ModelItem = modelItem;
            Groups = Find();
        }

        public ModelItem ModelItem { get; private set; }

        public InArgumentExpressionGroup[] Groups { get; private set; }

        private InArgumentExpressionGroup[] Find()
        {
            return (from prop in ModelItem.Properties
                    let mi = prop.Value
                    where mi != null
                    from cat in prop.Attributes.OfType<CategoryAttribute>().Select(a => a.Category)
                    where !string.IsNullOrEmpty(cat)
                    orderby cat
                    select cat)
                        .Distinct()
                        .Select(cat => new InArgumentExpressionGroup(ModelItem, cat))
                        .ToArray();
        }
    }
}
