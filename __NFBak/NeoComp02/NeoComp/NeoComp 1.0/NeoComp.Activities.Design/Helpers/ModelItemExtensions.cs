using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities.Presentation.Model;

namespace NeoComp.Activities.Design.Helpers
{
    public static class ModelItemExtensions
    {
        public static IEnumerable<ModelItem> GetParents(this ModelItem modelItem)
        {
            if (modelItem != null)
            {
                var parentModelItem = modelItem.Parent;
                while (parentModelItem != null)
                {
                    yield return parentModelItem;
                    parentModelItem = parentModelItem.Parent;
                }
            }
        }
    }
}
