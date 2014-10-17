using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Activities;
using System.Collections.ObjectModel;

namespace Neuroflow.Design.ActivityDesigners.Controls
{
    public sealed class PresentablePropertyTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ActivityPropertyTemplate { get; set; }

        public DataTemplate ActivityCollectionPropertyTemplate { get; set; }

        public DataTemplate DefaultTemplate { get; set; }
        
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var pp = item as PresentableProperty;

            if (pp != null)
            {
                var property = pp.ModelItem.Properties[pp.PropertyName];
                if (property != null)
                {
                    if (property.PropertyType.IsSubclassOf(typeof(ActivityWithResult))) return ActivityPropertyTemplate;
                    if (property.PropertyType == typeof(Collection<ActivityWithResult>)) return ActivityCollectionPropertyTemplate;
                }
            }
            
            return DefaultTemplate;
        }
    }
}
