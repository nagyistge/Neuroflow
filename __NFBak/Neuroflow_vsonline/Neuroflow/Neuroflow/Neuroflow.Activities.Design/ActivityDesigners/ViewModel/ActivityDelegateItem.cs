using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities.Presentation.Model;
using System.Diagnostics.Contracts;
using Neuroflow.Core.ComponentModel;

namespace Neuroflow.Activities.Design.ActivityDesigners.ViewModel
{
    public sealed class ActivityDelegateItem
    {
        public ActivityDelegateItem(ModelItem modelItem, ActivityDelegateMetadataAttribute metadata = null)
        {
            Contract.Requires(modelItem != null);
            ModelItem = modelItem;

            if (metadata == null)
            {
                var pmi = modelItem.Parent.Properties.Where(p => p.Value == modelItem).First();
                metadata = pmi.Attributes[typeof(ActivityDelegateMetadataAttribute)] as ActivityDelegateMetadataAttribute;
            }

            if (metadata != null)
            {
                ObjectName = metadata.ObjectName;
                ResultName = metadata.ResultName;
                Argument1Name = metadata.Argument1Name;
                Argument2Name = metadata.Argument2Name;
                Argument3Name = metadata.Argument3Name;
                Argument4Name = metadata.Argument4Name;
                Argument5Name = metadata.Argument5Name;
                Argument6Name = metadata.Argument6Name;
                Argument7Name = metadata.Argument7Name;
                Argument8Name = metadata.Argument8Name;
            }
            if (string.IsNullOrWhiteSpace(ObjectName)) ObjectName = ModelItem.ItemType.GetGenericArguments()[0].Name;
        }

        public ModelItem ModelItem { get; private set; }

        public string ObjectName { get; private set; }

        public string ResultName { get; private set; }

        public string Argument1Name { get; private set; }

        public string Argument2Name { get; private set; }

        public string Argument3Name { get; private set; }

        public string Argument4Name { get; private set; }

        public string Argument5Name { get; private set; }

        public string Argument6Name { get; private set; }

        public string Argument7Name { get; private set; }

        public string Argument8Name { get; private set; }
    }
}
