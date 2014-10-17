using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Activities;
using System.Windows;
using System.Activities.Presentation.Model;

namespace Neuroflow.Design.ActivityDesigners.Converters
{
    public sealed class IsVisualBasicValueToVisibility : IValueConverter
    {
        public bool Negate { get; set; }
        
        object IValueConverter.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var mi = value as ModelItem;
            bool isVB = mi != null && mi.ItemType.Name.ToLower().StartsWith("visualbasicvalue");
            if (Negate) isVB = !isVB;
            return isVB ? Visibility.Visible : Visibility.Collapsed;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
