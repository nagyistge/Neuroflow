using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Diagnostics.Contracts;
using System.Collections;

namespace NeoComp.Computations
{
    internal static class ToStringHelpers
    {
        internal static void AppendValues<T>(this StringBuilder sb, IEnumerable<T> values)
        {
            Contract.Requires(sb != null);
            Contract.Requires(values != null);

            foreach (var value in values)
            {
                if (sb.Length != 0) sb.Append(' ');
                sb.Append(value);
            }
        }
    }
}
