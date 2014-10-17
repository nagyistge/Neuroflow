using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Computations
{
    public abstract class NormalizedDoubleTransformer<T> : DoubleTransformer<T>
    {
        public NormalizedDoubleTransformer(DoubleNormalizer normalizer)
        {
            Contract.Requires(normalizer != null);

            Normalizer = normalizer;
        }
        
        public DoubleNormalizer Normalizer { get; private set; }

        public sealed override double Transform(T value)
        {
            return Normalizer.Transform(ToDouble(value));
        }

        protected abstract double ToDouble(T value);
    }
}
