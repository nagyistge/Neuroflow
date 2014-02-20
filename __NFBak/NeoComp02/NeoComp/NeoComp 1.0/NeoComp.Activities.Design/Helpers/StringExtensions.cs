using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.Activities.Design.Helpers
{
    public static class StringExtensions
    {
        public static string CamelCase(this string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                str = str.Trim();
                return char.ToLower(str[0]).ToString() + str.Substring(1);
            }
            return null;
        }
    }
}
