using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows;

namespace Neuroflow.Activities.Design.Converters
{
    public class VisConv : IValueConverter
    {
        public bool Neg { get; set; }

        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            bool visibility = !Neg ? (bool)value : !(bool)value;
            return visibility ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            Visibility visibility = (Visibility)value;
            return !Neg ? (visibility == Visibility.Visible) : !(visibility == Visibility.Visible);
        }
    } 
}
