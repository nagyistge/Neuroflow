using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Computations
{
    public static class Transformation
    {
        public static IEnumerable<TTo> Transform<TFrom, TTo>(this IEnumerable<TFrom> values, ITransformer<TFrom, TTo> transformer)
        {
            Contract.Requires(values != null);
            Contract.Requires(transformer != null);

            return values.Select(v => transformer.Transform(v));
        }
    }
}
