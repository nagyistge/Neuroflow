using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using NeoComp.Core;

namespace NeoComp.Computations
{
    public static class Vector
    {
        #region Wrap

        public static Vector<T> Wrap<T>(params T?[] items) where T : struct
        {
            return new Vector<T>(items, true);
        } 

        #endregion

        #region MSE

        public static double ComputeMeanSquareError(this Vector<double> vector1, Vector<double> vector2, double scale = 1.0)
        {
            Contract.Requires(vector1 != null);
            Contract.Requires(vector2 != null);
            Contract.Requires(vector1.Dimension == vector2.Dimension);
            Contract.Requires(scale > 0.0);
            Contract.Ensures(Contract.Result<double>() >= 0.0);

            return ComputeMeanSquareErrorInternal(vector1, vector2, scale);
        }

        internal static double ComputeMeanSquareErrorInternal(this Vector<double> vector1, Vector<double> vector2, double scale = 1.0)
        {
            Contract.Requires(vector1 != null);
            Contract.Requires(vector2 != null);
            Contract.Requires(vector1.Dimension == vector2.Dimension);
            Contract.Requires(scale > 0.0);
            Contract.Ensures(Contract.Result<double>() >= 0.0);

            double sum = 0.0;
            double count = 0.0;
            for (int idx = 0; idx < vector1.ItemArray.Length; idx++, count++)
            {
                double? value1 = vector1.ItemArray[idx];
                double? value2 = vector2.ItemArray[idx];
                if (value1.HasValue && value2.HasValue)
                {
                    sum += Math.Pow((value2.Value - value1.Value) * scale, 2.0);
                }
            }

            return (sum / count) * 0.5;
        }

        #endregion

        #region Diff

        public static IEnumerable<double?> ComputeDifference(this Vector<double> vector1, Vector<double> vector2)
        {
            Contract.Requires(vector1 != null);
            Contract.Requires(vector2 != null);
            Contract.Requires(vector1.Dimension == vector2.Dimension);
            Contract.Ensures(Contract.Result<IEnumerable<double?>>() != null);

            return ComputeDifferenceInternal(vector1, vector2);
        }

        internal static IEnumerable<double?> ComputeDifferenceInternal(this Vector<double> vector1, Vector<double> vector2)
        {
            Contract.Requires(vector1 != null);
            Contract.Requires(vector2 != null);
            Contract.Requires(vector1.Dimension == vector2.Dimension);

            for (int idx = 0; idx < vector1.ItemArray.Length; idx++)
            {
                double? value1 = vector1.ItemArray[idx];
                double? value2 = vector2.ItemArray[idx];
                if (value1.HasValue && value2.HasValue)
                {
                    yield return value2.Value - value1.Value;
                }
                else
                {
                    yield return null;
                }
            }
        }

        #endregion
    }
    
    public sealed class Vector<T> : ReadOnlyArray<T?>
        where T : struct
    {
        #region Constructors

        public Vector(params T?[] items)
            : base(items)
        {
        }

        internal Vector(T?[] items, bool wrap)
            : base(items, wrap)
        {
        }

        public Vector(IEnumerable<T?> items)
            : base(items)
        {
            Contract.Requires(items != null);
        }

        public Vector(IList<T?> items)
            : base(items)
        {
            Contract.Requires(items != null);
        }

        public Vector(IEnumerable<T> items)
            : base(items.Select(i => (T?)i).ToArray())
        {
            Contract.Requires(items != null);
        }

        public Vector(IList<T> items)
            : base(items.Select(i => (T?)i).ToArray())
        {
            Contract.Requires(items != null);
        } 

        #endregion

        #region Properties
        
        public int Dimension
        {
            [Pure]
            get { return ItemArray.Length; }
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var value in ItemArray)
            {
                if (sb.Length != 0) sb.Append(", ");
                if (value == null) sb.Append("null"); else sb.Append(value);
            }
            return sb.ToString();
        }

        #endregion
    }
}
