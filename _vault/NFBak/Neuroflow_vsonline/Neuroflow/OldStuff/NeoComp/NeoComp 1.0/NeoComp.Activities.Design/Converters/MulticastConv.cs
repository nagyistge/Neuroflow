using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace NeoComp.Activities.Design.Converters
{
    public sealed class MulticastConv : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values != null && values.Length != 0) return TrimConv.Trim(values[0]);
            return DependencyProperty.UnsetValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            string strPar = parameter as string;
            if (strPar != null)
            {
                int count;
                if (int.TryParse(strPar, out count) && count >= 0)
                {
                    value = TrimConv.Trim(value);
                    return Enumerable.Repeat(value, count).ToArray();
                }
            }
            return null;
        }
    }
}
