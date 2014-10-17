using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Computations;

namespace NeoComp.Computations
{
    public abstract class DoubleTransformer<T> : ITransformer<T, double>
    {
        public abstract double Transform(T value);
    }
}
