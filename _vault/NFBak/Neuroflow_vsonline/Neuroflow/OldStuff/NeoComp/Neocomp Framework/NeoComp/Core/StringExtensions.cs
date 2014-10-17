using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Core
{
    internal static class StringExtensions
    {
        [Pure]
        public static bool IsNullOrEmpty(this string value)
        {
            if (value == null) return true;
            if (value.Trim() == string.Empty) return true;
            return false;
        }

        [Pure]
        public static int GetHash(this string value)
        {
            if (value == null) return 0;
            return value.GetHashCode();
        }
    }
}
