using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp
{
    public struct IntRange : IEquatable<IntRange>, IRange<int>
    {
        #region Create

        public static IntRange CreateFixed(int value)
        {
            return new IntRange(value, value);
        }

        public static IntRange CreateInclusive(int minValue, int maxValueInclusive)
        {
            Contract.Requires(minValue <= maxValueInclusive);

            return new IntRange(minValue, maxValueInclusive);
        }

        public static IntRange CreateExclusive(int minValue, int maxValueExclusive)
        {
            Contract.Requires(minValue < maxValueExclusive);

            return new IntRange(minValue, maxValueExclusive - 1);
        }

        #endregion

        #region Constructors

        private IntRange(int minValue, int maxValue)
        {
            Contract.Requires(minValue <= maxValue);

            this.minValue = minValue;
            this.maxValue = maxValue;
        }

        #endregion

        #region Properties

        int minValue;

        public int MinValue
        {
            [Pure]
            get { return minValue; }
        }

        int maxValue;

        public int MaxValue
        {
            [Pure]
            get { return maxValue; }
        }

        public bool IsFixed
        {
            [Pure]
            get { return minValue == maxValue; }
        }

        public int Size
        {
            [Pure]
            get { return maxValue - minValue + 1; }
        }

        public bool IsPositive
        {
            [Pure]
            get { return minValue >= 0; }
        }

        public bool IsZero
        {
            [Pure]
            get { return minValue == 0 && maxValue == 0; }
        }

        #endregion

        #region Stuff

        public int PickRandomValue()
        {
            return IsFixed ? minValue : RandomGenerator.Random.Next(minValue, maxValue + 1);
        }

        [Pure]
        public bool IsIn(int value)
        {
            return value >= minValue && value <= maxValue;
        }

        public int Cut(int value)
        {
            if (value < minValue) return minValue;
            if (value > maxValue) return maxValue;
            return value;
        }

        #endregion

        #region IEquatable Members

        public bool Equals(IntRange other)
        {
            return minValue == other.minValue && maxValue == other.maxValue;
        }

        bool IEquatable<IRange<int>>.Equals(IRange<int> other)
        {
            if (other != null)
            {
                return minValue == other.MinValue && maxValue == other.MaxValue;
            }
            return false;
        }

        #endregion

        #region Object

        public override bool Equals(object obj)
        {
            return obj is IntRange ? Equals((IntRange)obj) : false;
        }

        public override int GetHashCode()
        {
            return minValue ^ maxValue;
        }

        public override string ToString()
        {
            return string.Format("{0}-{1}", minValue, maxValue);
        }

        #endregion

        #region Operators

        public static bool operator ==(IntRange range1, IntRange range2)
        {
            return range1.Equals(range2);
        }

        public static bool operator !=(IntRange range1, IntRange range2)
        {
            return !range1.Equals(range2);
        }

        #endregion
    }
}
