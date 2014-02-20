using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.Computations2
{
    public sealed class ComputationValue<T>
        where T : struct
    {
        public T Value { get; set; }
    }
}
