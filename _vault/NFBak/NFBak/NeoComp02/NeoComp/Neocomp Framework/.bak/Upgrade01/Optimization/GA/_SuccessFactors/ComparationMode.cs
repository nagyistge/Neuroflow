using System;

namespace NeoComp.Optimization.GA
{
    [Flags]
    public enum ComparationMode
    {
        None = 0,
        LowerIsBetter = 4,
        NullIsBetter = 8
    }
}