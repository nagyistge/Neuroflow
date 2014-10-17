using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Globalization;

namespace NeoComp.Features
{
    internal static class FeatureHelpers
    {
        internal static string GetID(string definition, out char? postfix)
        {
            Contract.Requires(!String.IsNullOrEmpty(definition));
            
            char last = definition[definition.Length - 1];
            switch (last)
            {
                case '<':
                case '>':
                case '#':
                    postfix = last;
                    return definition.Substring(0, definition.Length - 1);
                default:
                    postfix = null;
                    return definition;
            }
        }

        internal static double? ToDouble(object obj)
        {
            if (obj == null) return null;
            if (obj is double) return (double)obj;
            if (obj is double?) return (double?)obj;
            if (obj is string)
            {
                double value;
                if (double.TryParse((string)obj, NumberStyles.Float, CultureInfo.InvariantCulture, out value)) return value; return null;
            }
            return (double)Convert.ChangeType(obj, typeof(double));
        }
    }
}
