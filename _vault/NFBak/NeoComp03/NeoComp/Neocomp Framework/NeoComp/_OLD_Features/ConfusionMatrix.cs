using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Computations;
using System.Diagnostics.Contracts;
using NeoComp.Core;

namespace NeoComp.Features
{
    public sealed class ConfusionMatrix
    {
        public ConfusionMatrix(Matrix<double> actualMatrix, Matrix<double> predictedMatrix)
            : this(Compute(actualMatrix.ItemArray, predictedMatrix.ItemArray))
        {
            Contract.Requires(actualMatrix != null);
            Contract.Requires(predictedMatrix != null);
            Contract.Requires(actualMatrix.Width == predictedMatrix.Width);
            Contract.Requires(actualMatrix.Height == predictedMatrix.Height);
            Contract.Requires(actualMatrix.Width != 0);
            Contract.Requires(actualMatrix.Height != 0);
        }

        private ConfusionMatrix(int[,] data)
        {
            Contract.Requires(data != null);
            Contract.Requires(data.Rank == 2);
            Contract.Requires(data.GetUpperBound(0) == data.GetUpperBound(1));

            this.data = data;
        }

        public static ConfusionMatrix CreateFromContinousValue(Matrix<double> actualMatrix, Matrix<double> predictedMatrix, int max, int position = 0)
        {
            Contract.Requires(actualMatrix != null);
            Contract.Requires(predictedMatrix != null);
            Contract.Requires(actualMatrix.Width == predictedMatrix.Width);
            Contract.Requires(actualMatrix.Height == predictedMatrix.Height);
            Contract.Requires(actualMatrix.Width != 0);
            Contract.Requires(actualMatrix.Height != 0);
            Contract.Requires(position >= 0 && position < actualMatrix.Width);
            Contract.Requires(max > 0);

            var actList = new List<Vector<double>>(actualMatrix.Height);
            var predList = new List<Vector<double>>(actualMatrix.Height);
            for (int row = 0; row < actualMatrix.Height; row++)
            {
                var actVect = actualMatrix[row];
                var predVect = predictedMatrix[row];
                double? actValue = actVect[position];
                double? predValue = predVect[position];
                actList.Add(CreateVector(actValue, max));
                predList.Add(CreateVector(predValue, max));
            }

            var data = Compute(actList, predList);
            return new ConfusionMatrix(data);
        }

        private static Vector<double> CreateVector(double? actValue, int max)
        {
            int? idx = actValue == null ? default(int?) : (int)Math.Round((actValue.Value * 0.5 + 0.5) * (double)(max - 1));
            var a = new double?[max];
            if (idx.HasValue)
            {
                if (idx < 0) a[0] = 1.0;
                if (idx > max) a[max] = 1.0;
                for (int i = 0; i < max; i++)
                {
                    if (i == idx) a[i] = 1.0; else a[i] = 0.0;
                }
            }
            return Vector.Wrap(a);
        }

        int[,] data;

        public int Size
        {
            get { return data.GetUpperBound(0); }
        }

        public int this[int rowIndex, int colIndex]
        {
            get
            {
                Contract.Requires(rowIndex >= 0 && rowIndex < data.GetUpperBound(0));
                Contract.Requires(colIndex >= 0 && colIndex < data.GetUpperBound(1));
                Contract.Ensures(Contract.Result<int>() >= 0);
                
                return data[rowIndex, colIndex];
            }
        }

        public int GetCount(int colIndex)
        {
            Contract.Requires(colIndex >= 0 && colIndex < data.GetUpperBound(1));
            int c = 0;
            for (int idx = 0; idx < Size; idx++)
            {
                c += data[idx, colIndex];
            }
            return c;
        }

        private static int[,] Compute(IList<Vector<double>> actualVectors, IList<Vector<double>> predictedVectors)
        {
            int dim = actualVectors[0].Dimension;
            int count = actualVectors.Count;
            int[,] counts = new int[dim, dim];

            for (int idx = 0; idx < count; idx++)
            {
                var actual = actualVectors[idx];
                int? actIndex = GetIndex(actual);
                if (actIndex != null)
                {
                    var predicted = predictedVectors[idx];
                    int predIndex = GetIndex(predicted);
                    counts[predIndex, actIndex.Value]++;
                }
            }

            return counts;
        }

        private static int GetIndex(Vector<double> vector)
        {
            double? max = null;
            int maxIdx = 0;
            var a = vector.ItemArray;
            for (int idx = 0; idx < a.Length; idx++)
            {
                double? value = a[idx];
                if (value != null && (max == null || max.Value < value.Value))
                {
                    max = value;
                    maxIdx = idx;
                }
            }
            return max == null ? RandomGenerator.Random.Next(a.Length) : maxIdx;
        }
    }
}
