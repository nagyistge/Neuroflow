using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Neuroflow.Activities.Design.Converters
{
    [ValueConversion(typeof(string), typeof(string))]
    public sealed class TrimConv : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Trim(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Trim(value);
        }

        internal static object Trim(object value)
        {
            string str = value as string;
            return str != null ? str.Trim() : value;
        }
    }
}
