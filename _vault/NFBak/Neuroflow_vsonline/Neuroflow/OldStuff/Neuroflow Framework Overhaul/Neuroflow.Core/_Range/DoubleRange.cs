using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;

namespace Neuroflow.Core
{
    [Serializable]
    [DataContract(Namespace = xmlns.Neuroflow, Name = "dblRng")]
    public struct DoubleRange: IEquatable<DoubleRange>, IRange<double>
    {
        #region Create

        public static DoubleRange Create(double minValue, double size)
        {
            return new DoubleRange(minValue, minValue + size);
        }

        public static DoubleRange CreateSize(double size)
        {
            Contract.Requires(size >= 0.0);

            return new DoubleRange(0.0, size);
        }

        public static DoubleRange CreateFixed(double value)
        {
            return new DoubleRange(value, value);
        }

        #endregion

        #region Constructors

        public DoubleRange(double minValue, double maxValue)
        {
            Contract.Requires(minValue <= maxValue);

            this.minValue = minValue;
            this.maxValue = maxValue;
        } 

        #endregion

        #region Properties

        [DataMember(Name = "min")]
        double minValue;

        public double MinValue
        {
            [Pure]
            get { return minValue; }
        }

        [DataMember(Name = "max")]
        double maxValue;

        public double MaxValue
        {
            [Pure]
            get { return maxValue; }
        }

        public bool IsFixed
        {
            [Pure]
            get { return minValue == maxValue; }
        }

        public double Size
        {
            [Pure]
            get { return maxValue - minValue; }
        }

        public bool IsPositive
        {
            [Pure]
            get { return minValue >= 0.0; }
        }

        public bool IsZero
        {
            [Pure]
            get { return minValue == 0.0 && maxValue == 0.0; }
        }

        #endregion

        #region Stuff

        public double PickRandomValue()
        {
            return RandomGenerator.NextDouble(minValue, maxValue);
        }

        [Pure]
        public bool IsIn(double value)
        {
            return value >= minValue && value <= maxValue;
        }

        public double Cut(double value)
        {
            if (value < minValue) return minValue;
            if (value > maxValue) return maxValue;
            return value;
        }

        public double Normalize(double value, DoubleRange targetRange)
        {
            Contract.Requires(!IsFixed);
            Contract.Requires(!targetRange.IsFixed);
            Contract.Requires(IsIn(value));

            double targetDist = targetRange.maxValue - targetRange.minValue;
            double dist = maxValue - minValue;
            return (targetDist * (value - minValue)) / dist + targetRange.minValue;
        }

        public double Denormalize(double value, DoubleRange valueRange)
        {
            Contract.Requires(!IsFixed);
            Contract.Requires(!valueRange.IsFixed);
            Contract.Requires(valueRange.IsIn(value));

            double targetDist = valueRange.maxValue - valueRange.minValue;
            double dist = maxValue - minValue;
            return ((value - valueRange.minValue) * dist) / targetDist + minValue;
        }

        #endregion

        #region IEquatable Members

        public bool Equals(DoubleRange other)
        {
            return minValue == other.minValue && maxValue == other.maxValue;
        }

        bool IEquatable<IRange<double>>.Equals(IRange<double> other)
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
            return obj is DoubleRange ? Equals((DoubleRange)obj) : false;
        }

        public override int GetHashCode()
        {
            return minValue.GetHashCode() ^ maxValue.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0}-{1}", minValue, maxValue);
        }

        #endregion

        #region Operators

        public static bool operator ==(DoubleRange range1, DoubleRange range2)
        {
            return range1.Equals(range2);
        }

        public static bool operator !=(DoubleRange range1, DoubleRange range2)
        {
            return !range1.Equals(range2);
        }

        #endregion
    }
}
