using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeoComp
{
    public interface IRange<T> : IEquatable<IRange<T>>
        where T : struct
    {
        T MinValue { get; }

        T MaxValue { get; }

        T Size { get; }

        bool IsFixed { get; }

        bool IsPositive { get; }

        bool IsZero { get; }

        T PickRandomValue();

        bool IsIn(T value);

        T Cut(T value);
    }
}
