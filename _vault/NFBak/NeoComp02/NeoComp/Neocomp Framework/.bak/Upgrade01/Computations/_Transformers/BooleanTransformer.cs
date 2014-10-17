using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.Computations
{
    public abstract class BooleanTransformer<T> : ITransformer<T, bool>
    {
        public abstract bool Transform(T value);
    }
}
