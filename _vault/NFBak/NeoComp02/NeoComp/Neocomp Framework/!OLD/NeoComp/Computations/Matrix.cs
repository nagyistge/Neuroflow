using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Diagnostics.Contracts;

namespace NeoComp.Computations
{
    public static class Matrix
    {
        #region Wrap

        public static Matrix<T> Wrap<T>(params Vector<T>[] items) where T : struct
        {
            return new Matrix<T>(items, true);
        }

        #endregion

        #region MSE

        public static double ComputeMeanSquareError(this Matrix<double> referenceMatrix, Matrix<double> actualMatrix, double scale = 1.0)
        {
            Contract.Requires(referenceMatrix != null);
            Contract.Requires(actualMatrix != null);
            Contract.Requires(referenceMatrix.Width == actualMatrix.Width);
            Contract.Requires(referenceMatrix.Height == actualMatrix.Height);
            Contract.Requires(scale > 0.0);
            Contract.Ensures(Contract.Result<double>() >= 0.0);

            return ComputeMeanSquareErrorInternal(referenceMatrix, actualMatrix);
        }

        internal static double ComputeMeanSquareErrorInternal(this Matrix<double> referenceMatrix, Matrix<double> actualMatrix, double scale = 1.0)
        {
            Contract.Requires(referenceMatrix != null);
            Contract.Requires(actualMatrix != null);
            Contract.Requires(referenceMatrix.Width == actualMatrix.Width);
            Contract.Requires(referenceMatrix.Height == actualMatrix.Height);
            Contract.Requires(scale > 0.0);
            Contract.Ensures(Contract.Result<double>() >= 0.0);

            double sum = 0.0;
            double count = 0.0;
            for (int row = 0; row < referenceMatrix.Height; row++, count++)
            {
                var referenceVector = referenceMatrix[row];
                var actualVector = actualMatrix[row];
                sum += referenceVector.ComputeMeanSquareErrorInternal(actualVector, scale);
            }

            return sum / count;
        }

        #endregion
    }
    
    public sealed class Matrix<T> : ReadOnlyArray<Vector<T>>
        where T : struct
    {
        #region Constructors

        public Matrix(params Vector<T>[] items)
            : base(items)
        {
        }

        internal Matrix(Vector<T>[] items, bool wrap)
            : base(items, wrap)
        {
        }

        public Matrix(IEnumerable<Vector<T>> items)
            : base(items)
        {
            Contract.Requires(items != null);
        }

        public Matrix(IList<Vector<T>> items)
            : base(items)
        {
            Contract.Requires(items != null);
        }

        #endregion

        #region Properies

        public int Width
        {
            [Pure]
            get { return IsEmpty ? 0 : this[0].Dimension; }
        }

        public int Height
        {
            [Pure]
            get { return Count; }
        }

        #endregion

        #region Object

        public override string ToString()
        {
            if (IsEmpty) return "";
            var sb = new StringBuilder();
            foreach (var vector in this)
            {
                if (sb.Length != 0) sb.AppendLine();
                sb.Append(vector);
            }
            return sb.ToString();
        }

        #endregion
    }
}
