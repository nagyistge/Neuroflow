using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Computations
{
    public sealed class NullableTransformer<TFrom, TTo> : ITransformer<TFrom?, TTo?>
        where TFrom : struct
        where TTo : struct
    {
        public NullableTransformer(ITransformer<TFrom, TTo> valueConverter)
        {
            Contract.Requires(valueConverter != null);
            this.valueConverter = valueConverter;
        }

        ITransformer<TFrom, TTo> valueConverter;

        public TTo? Transform(TFrom? value)
        {
            return value.HasValue ? valueConverter.Transform(value.Value) : default(TTo?);
        }
    }
}
