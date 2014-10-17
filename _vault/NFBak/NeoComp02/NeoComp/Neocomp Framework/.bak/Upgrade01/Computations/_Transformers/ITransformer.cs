using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp.Computations
{
    public interface ITransformer<TFrom, TTo>
    {
        TTo Transform(TFrom value);
    }
}
