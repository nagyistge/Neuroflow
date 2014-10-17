using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Computations
{
    public interface IRealVectorizer<T> : IVectorizer<double, T>
        where T : class
    {
    }
}
