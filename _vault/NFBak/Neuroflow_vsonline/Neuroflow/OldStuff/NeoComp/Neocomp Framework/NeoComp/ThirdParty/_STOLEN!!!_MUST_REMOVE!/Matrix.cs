using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ThirdParty
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Matrix
    {
        private const char ShouldBe_Environment_NewLine = '\n';
        public double[] data;
        private int r;
        private int c;
        private bool t;
        private double noiseLevel;
        private static Random rand;
        public static readonly int End;
        public static Random rgen;
        public bool IsSquare
        {
            get
            {
                return (this.r == this.c);
            }
        }
        public Matrix(int rows, int cols)
        {
            if (rows < 0)
            {
                throw new ArgumentException("rows < 0");
            }
            if (cols < 0)
            {
                throw new ArgumentException("cols < 0");
            }
            this.t = false;
            this.r = rows;
            this.c = cols;
            this.data = new double[this.r * this.c];
            this.noiseLevel = 0.1;
        }

        public Matrix(int rows, int cols, double fill)
            : this(rows, cols)
        {
            this.Fill(fill);
        }

        public Matrix(int rows, int cols, double[] data, bool transp)
            : this(rows, cols, data, transp, 0.1)
        {
        }

        public Matrix(int rows, int cols, double[] data, bool transp, double noise)
        {
            if ((rows * cols) != ((data == null) ? 0 : data.Length))
            {
                throw new ArgumentException(string.Concat(new object[] { "data does not have Length == R*C = ", rows * cols, ". you tried ", data.Length }));
            }
            this.noiseLevel = noise;
            this.t = transp;
            this.r = rows;
            this.c = cols;
            this.data = data;
        }

        public Matrix(params double[] rowData)
            : this(1, rowData.Length, rowData, false)
        {
        }

        public Matrix(double[][] array)
        {
            this = new Matrix(array.Length, array[0].Length);
            for (int i = 0; i < array.Length; i++)
            {
                Array.Copy(array[i], 0, this.data, i * array[i].Length, array[i].Length);
            }
        }

        public int Rows
        {
            get
            {
                return this.r;
            }
        }
        public int Columns
        {
            get
            {
                return this.c;
            }
        }
        public static int RowCount(Matrix[] matrix)
        {
            int num = 0;
            foreach (Matrix matrix2 in matrix)
            {
                num += matrix2.r;
            }
            return num;
        }

        public static int ColCount(Matrix[] matrix)
        {
            int num = 0;
            foreach (Matrix matrix2 in matrix)
            {
                num += matrix2.c;
            }
            return num;
        }

        public void CopyTo(double[] d, int pos)
        {
            if ((d.Length - pos) < this.data.Length)
            {
                throw new ArgumentException("Array is not long enough. must be at least (Rows*Columns + pos) long");
            }
            Array.Copy(this.data, 0, d, pos, this.data.Length);
        }

        public double[] Data
        {
            get
            {
                if (this.data != null)
                {
                    return (double[])this.data.Clone();
                }
                return null;
            }
        }
        public double[][] ToJaggedArray()
        {
            double[][] numArray = new double[this.r][];
            for (int i = 0; i < this.r; i++)
            {
                double[] numArray2;
                numArray[i] = numArray2 = new double[this.c];
                Array.Copy(this.data, i * this.c, numArray2, 0, this.c);
            }
            return numArray;
        }

        public void FromJaggedArray(double[][] array)
        {
            this.r = array.Length;
            this.c = array[0].Length;
            this.data = new double[this.r * this.c];
            for (int i = 0; i < array.Length; i++)
            {
                Array.Copy(array[i], 0, this.data, i * array[i].Length, array[i].Length);
            }
        }

        public double this[int matrixAsSingleRow_Index]
        {
            get
            {
                return this.data[matrixAsSingleRow_Index];
            }
            set
            {
                this.data[matrixAsSingleRow_Index] = value;
            }
        }
        public double this[int row, int col]
        {
            get
            {
                if (((this.c <= col) || (this.r <= row)) || ((col < 0) || (row < 0)))
                {
                    throw new ArgumentOutOfRangeException(string.Concat(new object[] { "Matrix dimensions: ", this.c, "x", this.r, "You tried: [", row, " ", col, "]" }));
                }
                return this.data[(row * this.c) + col];
            }
            set
            {
                if (((this.c <= col) || (this.r <= row)) || ((col < 0) || (row < 0)))
                {
                    throw new ArgumentOutOfRangeException(string.Concat(new object[] { "Matrix dimensions: ", this.c, "x", this.r, "You tried: [", row, " ", col, "]" }));
                }
                this.data[(row * this.c) + col] = value;
            }
        }
        public Matrix this[bool row, int[] index]
        {
            get
            {
                double[] numArray;
                int length = index.Length;
                if (row)
                {
                    for (int k = 0; k < length; k++)
                    {
                        if ((index[k] < 0) || (index[k] >= this.r))
                        {
                            throw new ArgumentException("??? Index exceeds matrix dimensions. \n\tYou tried " + index[k], "index[" + k + "]");
                        }
                    }
                    numArray = new double[length * this.c];
                    for (int m = 0; m < length; m++)
                    {
                        Array.Copy(this.data, index[m] * this.c, numArray, m * this.c, this.c);
                    }
                    return new Matrix(length, this.c, numArray, false, this.noiseLevel);
                }
                for (int i = 0; i < length; i++)
                {
                    if ((index[i] < 0) || (index[i] >= this.c))
                    {
                        throw new ArgumentException("??? Index exceeds matrix dimensions.", string.Concat(new object[] { "index[", i, "] = ", index[i] }));
                    }
                }
                numArray = new double[length * this.r];
                for (int j = 0; j < length; j++)
                {
                    int num6 = 0;
                    for (int n = index[j]; num6 < this.r; n += this.c)
                    {
                        numArray[j + (num6++ * length)] = this.data[n];
                    }
                }
                return new Matrix(this.r, length, numArray, false, this.noiseLevel);
            }
            set
            {
                int length = index.Length;
                if (row)
                {
                    if (value.c != this.c)
                    {
                        throw new ArgumentException("???  In an assignment  A(true, int[]) = B, the number of columns in A and B must be the same.");
                    }
                    if (value.r != length)
                    {
                        throw new ArgumentException("???  In an assignment  A(true, int[]) = B, the number of elements in the subscript of A and the number of rows in B must be the same.");
                    }
                    for (int i = 0; i < length; i++)
                    {
                        if ((index[i] < 0) || (index[i] >= this.r))
                        {
                            throw new ArgumentException("??? Index exceeds matrix dimensions.", "index[" + i + "]");
                        }
                    }
                    for (int j = 0; j < length; j++)
                    {
                        Array.Copy(value.data, j * this.c, this.data, index[j] * this.c, this.c);
                    }
                }
                else
                {
                    if (value.r != this.r)
                    {
                        throw new ArgumentException("???  In an assignment  A(false, int[]) = B, the number of rows in A and B must be the same.");
                    }
                    if (value.c != length)
                    {
                        throw new ArgumentException("???  In an assignment  A(false, int[]) = B, the number of elements in the subscript of A and the number of columns in B must be the same.");
                    }
                    for (int k = 0; k < length; k++)
                    {
                        if ((index[k] < 0) || (index[k] >= this.c))
                        {
                            throw new ArgumentException("??? Index exceeds matrix dimensions.", "index[" + k + "]");
                        }
                    }
                    for (int m = 0; m < length; m++)
                    {
                        int num6 = 0;
                        for (int n = index[m]; num6 < this.r; n += this.c)
                        {
                            this.data[n] = value[m + (length * num6++)];
                        }
                    }
                }
            }
        }
        public Matrix this[int fromRow, int rows, int fromCol, int columns]
        {
            get
            {
                this.ValidateThis(fromRow, ref rows, fromCol, ref columns);
                double[] destinationArray = new double[rows * columns];
                for (int i = 0; i < rows; i++)
                {
                    Array.Copy(this.data, ((fromRow + i) * this.c) + fromCol, destinationArray, i * columns, columns);
                }
                return new Matrix(rows, columns, destinationArray, false, this.noiseLevel);
            }
            set
            {
                this.ValidateThis(fromRow, ref rows, fromCol, ref columns);
                int num = 0;
                int num2 = fromRow + rows;
                for (int i = fromRow; i < num2; i++)
                {
                    Array.Copy(value.data, num++ * value.c, this.data, (i * this.c) + fromCol, value.c);
                }
            }
        }
        private void ValidateThis(int fr, ref int rc, int fc, ref int cc)
        {
            if ((fr < 0) || (fr >= this.r))
            {
                throw new ArgumentOutOfRangeException("fromRow=" + fr, "0 <= fromRow < .Rows");
            }
            if ((rc < 0) || (rc > (this.r - fr)))
            {
                if (rc != -1)
                {
                    throw new ArgumentOutOfRangeException("toRow=" + ((int)rc), "0 <= fromRow < .Rows - rows");
                }
                rc = this.r - fr;
            }
            if ((fc < 0) || (fc >= this.c))
            {
                throw new ArgumentOutOfRangeException("fromCol=" + fc, "0 <= fromCol < .Columns");
            }
            if ((cc < 0) || (cc > (this.c - fc)))
            {
                if (cc != -1)
                {
                    throw new ArgumentOutOfRangeException("toCol=" + ((int)cc), "0 <= fromCol < .Columns - columns");
                }
                cc = this.c - fc;
            }
        }

        public Matrix MultipleRows(int fromRow, int rows)
        {
            return this[fromRow, rows, 0, this.c];
        }

        public Matrix MultipleColumns(int fromCol, int columns)
        {
            return this[0, this.r, fromCol, (columns == End) ? this.c : columns];
        }

        public Matrix GetRows(int[] idx)
        {
            int num4;
            if (idx.Length == this.r)
            {
                return this.MClone();
            }
            if (idx.Length > this.r)
            {
                throw new ArgumentException("??? Error Get Row(s) \nYou can't get more rows than you've got.");
            }
            Array.Sort<int>(idx);
            if (idx[0] < 0)
            {
                throw new ArgumentException("??? Error Get Row(s) \nIndex cannot be negative");
            }
            int length = idx.Length;
            if (idx[length - 1] > (this.r - 1))
            {
                throw new ArgumentException("??? Error Get Row(s) \nIndex exceeds matrix dimensions");
            }
            Matrix matrix = new Matrix(idx.Length, this.c);
            int index = 0;
            for (int i = 0; index < length; i += num4 * this.c)
            {
                num4 = 1;
                int num5 = idx[index];
                while (((index + num4) < length) && (idx[index + num4] == (num5 + num4)))
                {
                    num4++;
                }
                Array.Copy(this.data, num5 * this.c, matrix.data, i, num4 * this.c);
                index += num4;
            }
            return matrix;
        }

        public Matrix GetSortedRows(int[] sortIdx)
        {
            int num4;
            if (sortIdx.Length > this.r)
            {
                throw new ArgumentException("??? Error Get Row(s) \nYou can't get more rows than you've got.");
            }
            if (sortIdx[0] < 0)
            {
                throw new ArgumentException("??? Error Get Row(s) \nIndex cannot be negative");
            }
            int length = sortIdx.Length;
            if (sortIdx[length - 1] > (this.r - 1))
            {
                throw new ArgumentException("??? Error Get Row(s) \nIndex exceeds matrix dimensions");
            }
            if (sortIdx.Length == this.r)
            {
                return this.MClone();
            }
            Matrix matrix = new Matrix(sortIdx.Length, this.c);
            int index = 0;
            for (int i = 0; index < length; i += num4 * this.c)
            {
                num4 = 1;
                int num5 = sortIdx[index];
                while (((index + num4) < length) && (sortIdx[index + num4] == (num5 + num4)))
                {
                    num4++;
                }
                Array.Copy(this.data, num5 * this.c, matrix.data, i, num4 * this.c);
                index += num4;
            }
            return matrix;
        }

        public Matrix GetColumns(int[] sortIdx)
        {
            if (sortIdx.Length > this.c)
            {
                throw new ArgumentException("??? Error Get Column(s) \nYou can't get more columns than you've got.");
            }
            if (sortIdx[0] < 0)
            {
                throw new ArgumentException("??? Error Get Column(s) \nIndex cannot be negative");
            }
            int length = sortIdx.Length;
            if (sortIdx[length - 1] > (this.c - 1))
            {
                throw new ArgumentException("??? Error Get Column(s) \nIndex exceeds matrix dimensions");
            }
            if (sortIdx.Length == this.c)
            {
                return this.MClone();
            }
            Matrix matrix = new Matrix(sortIdx.Length, this.r);
            int index = 0;
            int destinationIndex = 0;
            int num6 = 0;
            while (num6 < this.r)
            {
                while (index < length)
                {
                    int num4 = 1;
                    int num5 = sortIdx[index];
                    while (((index + num4) < length) && (sortIdx[index + num4] == (num5 + num4)))
                    {
                        num4++;
                    }
                    Array.Copy(this.data, num5 + (num6 * this.c), matrix.data, destinationIndex, num4);
                    index += num4;
                    destinationIndex += num4;
                }
                num6++;
                destinationIndex = num6 * length;
            }
            return matrix;
        }

        public Matrix Plus(double d)
        {
            int num = 0;
            while (num < this.data.Length)
            {
                this.data[num++] += d;
            }
            return this;
        }

        public Matrix Minus(double d)
        {
            int num = 0;
            while (num < this.data.Length)
            {
                this.data[num++] -= d;
            }
            return this;
        }

        public Matrix PreMinus(double d)
        {
            int index = 0;
            while (index < this.data.Length)
            {
                this.data[index] = d - this.data[index++];
            }
            return this;
        }

        public Matrix Plus(Matrix m)
        {
            if ((this.r != m.r) || (this.c != m.c))
            {
                if ((m.r == 1) && (m.c == 1))
                {
                    return this.Plus(m.data[0]);
                }
                if ((m.r == 1) && (m.c == this.c))
                {
                    int num = 0;
                    while (num < this.r)
                    {
                        int num2 = num++ * this.c;
                        int num3 = 0;
                        while (num3 < this.c)
                        {
                            this.data[num2 + num3] += m.data[num3++];
                        }
                    }
                }
                else
                {
                    if ((m.r != this.r) || (m.c != 1))
                    {
                        throw new ArgumentException("??? Error using ==> +\nMatrix dimensions must agree.");
                    }
                    for (int i = 0; i < this.r; i++)
                    {
                        int num5 = i * this.c;
                        int num6 = 0;
                        while (num6 < this.c)
                        {
                            this.data[num5 + num6++] += m.data[i];
                        }
                    }
                }
                return this;
            }
            int index = 0;
            while (index < this.data.Length)
            {
                this.data[index] += m.data[index++];
            }
            return this;
        }

        public Matrix Minus(Matrix m)
        {
            if ((this.r != m.r) || (this.c != m.c))
            {
                if ((m.r == 1) && (m.c == 1))
                {
                    return this.Minus(m.data[0]);
                }
                if ((m.r == 1) && (m.c == this.c))
                {
                    int num = 0;
                    while (num < this.r)
                    {
                        int num2 = num++ * this.c;
                        int num3 = 0;
                        while (num3 < this.c)
                        {
                            this.data[num2 + num3] -= m.data[num3++];
                        }
                    }
                }
                else
                {
                    if ((m.r != this.r) || (m.c != 1))
                    {
                        throw new ArgumentException("??? Error using ==> -\nMatrix dimensions must agree.");
                    }
                    for (int i = 0; i < this.r; i++)
                    {
                        int num5 = i * this.c;
                        int num6 = 0;
                        while (num6 < this.c)
                        {
                            this.data[num5 + num6++] -= m.data[i];
                        }
                    }
                }
                return this;
            }
            int index = 0;
            while (index < this.data.Length)
            {
                this.data[index] -= m.data[index++];
            }
            return this;
        }

        public Matrix PreMinus(Matrix m)
        {
            if ((this.r != m.r) || (this.c != m.c))
            {
                if ((m.r == 1) && (m.c == 1))
                {
                    return this.Minus(m.data[0]);
                }
                if ((m.r == 1) && (m.c == this.c))
                {
                    int num = 0;
                    while (num < this.r)
                    {
                        int num2 = num++ * this.c;
                        int num3 = 0;
                        while (num3 < this.c)
                        {
                            this.data[num2 + num3] = m.data[num3] - this.data[num2 + num3++];
                        }
                    }
                }
                else
                {
                    if ((m.r != this.r) || (m.c != 1))
                    {
                        throw new ArgumentException("??? Error using ==> -\nMatrix dimensions must agree.");
                    }
                    for (int i = 0; i < this.r; i++)
                    {
                        int num5 = i * this.c;
                        int num6 = 0;
                        while (num6 < this.c)
                        {
                            this.data[num5 + num6] = m.data[i] - this.data[num5 + num6++];
                        }
                    }
                }
                return this;
            }
            int index = 0;
            while (index < this.data.Length)
            {
                this.data[index] = m.data[index] - this.data[index++];
            }
            return this;
        }

        public Matrix DivideBy(Matrix m)
        {
            if ((this.r != m.r) && (this.c != m.c))
            {
                throw new ArgumentException(string.Concat(new object[] { "??? Error using ==> ./", '\n', "   Matrix dimensions must agree.", '\n', "   You tried [", this.r, "\x00d7", this.c, "]./[", m.r, "\x00d7", m.c, "].", '\n', "   --> Try [r\x00d7c]./[r\x00d7c], [r\x00d7c]./[1\x00d7c] or [r\x00d7c]./[r\x00d71]" }));
            }
            if (this.r != m.r)
            {
                if (m.r != 1)
                {
                    throw new ArgumentException(string.Concat(new object[] { "??? Error using ==> ./", '\n', "   Matrix dimensions must agree.", '\n', "   You tried [", this.r, "\x00d7", this.c, "]./[", m.r, "\x00d7", m.c, "].", '\n', "   --> Try [r\x00d7c]./[r\x00d7c], [r\x00d7c]./[1\x00d7c] or [r\x00d7c]./[r\x00d71]" }));
                }
                int index = 0;
                while (index < this.data.Length)
                {
                    this.data[index] /= m.data[index++ % this.c];
                }
            }
            else if (this.c != m.c)
            {
                if (m.c != 1)
                {
                    throw new ArgumentException(string.Concat(new object[] { "??? Error using ==> ./", '\n', "   Matrix dimensions must agree.", '\n', "   You tried [", this.r, "\x00d7", this.c, "]./[", m.r, "\x00d7", m.c, "].", '\n', "   --> Try [r\x00d7c]./[r\x00d7c], [r\x00d7c]./[1\x00d7c] or [r\x00d7c]./[r\x00d71]" }));
                }
                int num2 = 0;
                while (num2 < this.data.Length)
                {
                    this.data[num2] /= m.data[num2++ / this.c];
                }
            }
            else
            {
                int num3 = 0;
                while (num3 < this.data.Length)
                {
                    this.data[num3] /= m.data[num3++];
                }
            }
            return this;
        }

        public Matrix DivideBy(double d)
        {
            int num = 0;
            while (num < this.data.Length)
            {
                this.data[num++] /= d;
            }
            return this;
        }

        public Matrix Pointprod(Matrix m)
        {
            if ((this.r != m.r) && (this.c != m.c))
            {
                throw new ArgumentException(string.Concat(new object[] { "??? Error using ==> .*", '\n', "   Matrix dimensions must agree.", '\n', "   You tried [", this.r, "\x00d7", this.c, "].*[", m.r, "\x00d7", m.c, "].", '\n', "   --> Try [r\x00d7c].*[r\x00d7c], [r\x00d7c].*[1\x00d7c] or [r\x00d7c].*[r\x00d71]" }));
            }
            if (this.r != m.r)
            {
                if (m.r != 1)
                {
                    throw new ArgumentException(string.Concat(new object[] { "??? Error using ==> .*", '\n', "   Matrix dimensions must agree.", '\n', "   You tried [", this.r, "\x00d7", this.c, "].*[", m.r, "\x00d7", m.c, "].", '\n', "   --> Try [r\x00d7c].*[r\x00d7c], [r\x00d7c].*[1\x00d7c] or [r\x00d7c].*[r\x00d71]" }));
                }
                int index = 0;
                while (index < this.data.Length)
                {
                    this.data[index] *= m.data[index++ % this.c];
                }
            }
            else if (this.c != m.c)
            {
                if (m.c != 1)
                {
                    throw new ArgumentException(string.Concat(new object[] { "??? Error using ==> .*", '\n', "   Matrix dimensions must agree.", '\n', "   You tried [", this.r, "\x00d7", this.c, "].*[", m.r, "\x00d7", m.c, "].", '\n', "   --> Try [r\x00d7c].*[r\x00d7c], [r\x00d7c].*[1\x00d7c] or [r\x00d7c].*[r\x00d71]" }));
                }
                int num2 = 0;
                while (num2 < this.data.Length)
                {
                    this.data[num2] *= m.data[num2++ / this.c];
                }
            }
            else
            {
                int num3 = 0;
                while (num3 < this.data.Length)
                {
                    this.data[num3] *= m.data[num3++];
                }
            }
            return this;
        }

        public Matrix Product(Matrix m)
        {
            if (this.c != m.r)
            {
                throw new ArgumentException(string.Concat(new object[] { "??? Error using ==> *\nMatrix dimensions must agree.\nYou tried [", this.r, "\x00d7", this.c, "]*[", m.r, "\x00d7", m.c, "].\n--> Try [r\x00d7n]*[n\x00d7c]" }));
            }
            Matrix matrix = new Matrix(this.r, m.c);
            for (int i = 0; i < this.r; i++)
            {
                for (int j = 0; j < m.c; j++)
                {
                    int index = (i * m.c) + j;
                    int num4 = i * this.c;
                    for (int k = 0; k < this.c; k++)
                    {
                        matrix.data[index] += this.data[num4 + k] * m.data[(k * m.c) + j];
                    }
                }
            }
            return matrix;
        }

        public Matrix Product(double d)
        {
            int num = 0;
            while (num < this.data.Length)
            {
                this.data[num++] *= d;
            }
            return this;
        }

        public Matrix Fill(double d)
        {
            int num = 0;
            while (num < this.data.Length)
            {
                this.data[num++] = d;
            }
            return this;
        }

        public static Matrix Enumerate(int rows)
        {
            Matrix matrix = new Matrix(rows, 1);
            for (int i = 0; i < rows; i++)
            {
                matrix.data[i] = i;
            }
            return matrix;
        }

        public Matrix Randomize()
        {
            if (this.noiseLevel == 0.0)
            {
                this.noiseLevel = 0.1;
            }
            int num = 0;
            while (num < this.data.Length)
            {
                this.data[num++] = this.noiseLevel * (2.0 * (rgen.NextDouble() - 0.5));
            }
            return this;
        }

        public Matrix Randomize(int seed)
        {
            Random random = new Random(seed);
            if (this.noiseLevel == 0.0)
            {
                this.noiseLevel = 0.1;
            }
            int num = 0;
            while (num < this.data.Length)
            {
                this.data[num++] = this.noiseLevel * (2.0 * (random.NextDouble() - 0.5));
            }
            return this;
        }

        public static Matrix TransposeOf(Matrix m)
        {
            if ((m.r != 1) && (m.c != 1))
            {
                double[] destinationArray = new double[m.r * m.c];
                for (int i = 0; i < m.c; i++)
                {
                    Array.Copy(m[false, new int[] { i }].data, 0, destinationArray, i * m.r, m.r);
                }
                return new Matrix(m.c, m.r, destinationArray, false, m.noiseLevel);
            }
            Matrix matrix = m.MClone();
            matrix.r = m.c;
            matrix.c = m.r;
            return matrix;
        }

        public Matrix Transpose()
        {
            if ((this.r != 1) && (this.c != 1))
            {
                Matrix matrix;
                this = matrix = TransposeOf(this);
                return matrix;
            }
            int c = this.c;
            this.c = this.r;
            this.r = c;
            return this;
        }

        public static Matrix Sum(bool rows, Matrix m)
        {
            Matrix matrix;
            matrix.noiseLevel = m.noiseLevel;
            matrix.t = m.t;
            if (rows)
            {
                matrix.data = new double[matrix.c = m.c];
                for (int j = 0; j < m.r; j++)
                {
                    for (int k = 0; k < m.c; k++)
                    {
                        matrix.data[k] += m.data[(j * m.c) + k];
                    }
                }
                matrix.r = 1;
                return matrix;
            }
            matrix.data = new double[matrix.r = m.r];
            for (int i = 0; i < m.r; i++)
            {
                for (int n = 0; n < m.c; n++)
                {
                    matrix.data[i] += m.data[(i * m.c) + n];
                }
            }
            matrix.c = 1;
            return matrix;
        }

        public Matrix Sum(bool rows)
        {
            Matrix matrix;
            this = matrix = Sum(rows, this);
            return matrix;
        }

        public static double Sum(Matrix M)
        {
            double num = 0.0;
            foreach (double num2 in M.data)
            {
                num += num2;
            }
            return num;
        }

        public double Sum()
        {
            return Sum(this);
        }

        public static Matrix Prod(bool rows, Matrix m)
        {
            Matrix matrix;
            matrix.noiseLevel = m.noiseLevel;
            matrix.t = m.t;
            if (rows)
            {
                matrix.data = new double[matrix.c = m.c];
                Array.Copy(m.data, matrix.data, matrix.data.Length);
                for (int k = 1; k < m.r; k++)
                {
                    int num2 = k * m.c;
                    for (int n = 0; n < m.c; n++)
                    {
                        matrix.data[n] *= m.data[num2 + n];
                    }
                }
                matrix.r = 1;
                return matrix;
            }
            matrix.data = new double[matrix.r = m.r];
            for (int i = 0; i < m.r; i++)
            {
                matrix.data[i] = m.data[i * m.c];
            }
            for (int j = 0; j < m.r; j++)
            {
                int num6 = j * m.c;
                for (int num7 = 1; num7 < m.c; num7++)
                {
                    matrix.data[j] *= m.data[num6 + num7];
                }
            }
            matrix.c = 1;
            return matrix;
        }

        public Matrix Prod(bool rows)
        {
            Matrix matrix;
            this = matrix = Prod(rows, this);
            return matrix;
        }

        public static double Prod(Matrix M)
        {
            double num = 1.0;
            foreach (double num2 in M.data)
            {
                num *= num2;
            }
            return num;
        }

        public double Prod()
        {
            return Prod(this);
        }

        public static double Trace(Matrix m)
        {
            int num = (m.r < m.c) ? m.r : m.c;
            double num2 = 1.0;
            for (int i = 0; i < num; i++)
            {
                num2 *= m.data[(i * num) + i];
            }
            return num2;
        }

        public double Trace()
        {
            return Trace(this);
        }

        public static Matrix DropRows(Matrix m, params int[] idx)
        {
            if (idx.Length >= m.r)
            {
                throw new ArgumentException("??? Error Removing Row(s) \nYou can't remove all or more columns than you've got.");
            }
            if (idx.Length == 0)
            {
                return m;
            }
            for (int i = 0; i < idx.Length; i++)
            {
                if (idx[i] > (m.r - 1))
                {
                    throw new ArgumentException("??? Error Removing Row(s) \nIndex exceeds matrix dimensions");
                }
                if (idx[i] < 0)
                {
                    throw new ArgumentException("??? Error Removing Row(s) \nIndex cannot be negative");
                }
            }
            Matrix matrix = m;
            matrix.r -= idx.Length;
            matrix.data = new double[matrix.r * matrix.c];
            Array.Sort<int>(idx);
            int index = idx.Length - 1;
            int destinationIndex = 0;
            int num3 = -1;
            while ((num3 < index) && (idx[num3 + 1] == (num3 + 1)))
            {
                num3++;
            }
            if (matrix.c == 1)
            {
                if (num3 < 0)
                {
                    Array.Copy(m.data, ++num3, matrix.data, destinationIndex, destinationIndex = idx[num3]);
                    while ((num3 < index) && ((idx[num3] + 1) == idx[num3 + 1]))
                    {
                        num3++;
                    }
                }
                while (num3 < index)
                {
                    int num5;
                    Array.Copy(m.data, idx[num3] + 1, matrix.data, destinationIndex, num5 = (-1 - idx[num3]) + idx[++num3]);
                    while ((num3 < index) && ((idx[num3] + 1) == idx[num3 + 1]))
                    {
                        num3++;
                    }
                    destinationIndex += num5;
                }
                if ((num3 = idx[index] + 1) < m.r)
                {
                    Array.Copy(m.data, num3, matrix.data, destinationIndex, m.r - num3);
                }
                return matrix;
            }
            if (num3 < 0)
            {
                Array.Copy(m.data, ++num3, matrix.data, destinationIndex, destinationIndex = idx[num3] * m.c);
                while ((num3 < index) && ((idx[num3] + 1) == idx[num3 + 1]))
                {
                    num3++;
                }
            }
            while (num3 < index)
            {
                int num6;
                Array.Copy(m.data, m.c * (idx[num3] + 1), matrix.data, destinationIndex, num6 = m.c * ((-1 - idx[num3]) + idx[++num3]));
                while ((num3 < index) && ((idx[num3] + 1) == idx[num3 + 1]))
                {
                    num3++;
                }
                destinationIndex += num6;
            }
            if ((num3 = idx[index] + 1) < m.r)
            {
                Array.Copy(m.data, m.c * num3, matrix.data, destinationIndex, m.c * (m.r - num3));
            }
            return matrix;
        }

        public static Matrix DropColumns(Matrix m, params int[] idx)
        {
            for (int i = 0; i < idx.Length; i++)
            {
                if (idx[i] > (m.c - 1))
                {
                    throw new ArgumentException("??? Error Removing Column(s) \nIndex exceeds matrix dimensions");
                }
                if (idx.Length > m.c)
                {
                    throw new ArgumentException("??? Error Removing Column(s) \nYou can't remove more columns than you've got.");
                }
                if (idx[i] < 0)
                {
                    throw new ArgumentException("??? Error Removing Column(s) \nIndex cannot be negative");
                }
            }
            Matrix matrix = m;
            matrix.data = new double[m.r * (matrix.c = m.c - idx.Length)];
            if (matrix.c == 0)
            {
                matrix.r = 0;
                return matrix;
            }
            int index = 0;
            int num3 = 0;
            while (index < m.data.Length)
            {
                bool flag = false;
                for (int j = 0; j < idx.Length; j++)
                {
                    if ((index % m.c) == idx[j])
                    {
                        flag = true;
                    }
                }
                if (!flag)
                {
                    matrix.data[num3++] = m.data[index];
                }
                index++;
            }
            return matrix;
        }

        public Matrix RemoveColumns(params int[] idx)
        {
            Matrix matrix;
            this = matrix = DropColumns(this, idx);
            return matrix;
        }

        public static void MoveCol(Matrix m, int from, int to)
        {
            if ((m.Rows != 0) && (from != to))
            {
                int num5;
                int num6;
                int num7;
                int num = 8;
                int num3 = 0;
                int num4 = 0;
                if (from > to)
                {
                    num5 = num * (from - to);
                    num6 = num * to;
                    num7 = num * (to + 1);
                }
                else
                {
                    num5 = num * (to - from);
                    num6 = num * (from + 1);
                    num7 = num * from;
                }
                int num8 = 0;
                while (num8 < m.r)
                {
                    double num2 = m.data[num3 + from];
                    Buffer.BlockCopy(m.data, num4 + num6, m.data, num4 + num7, num5);
                    m.data[num3 + to] = num2;
                    num8++;
                    num3 += m.c;
                    num4 = num * num3;
                }
            }
        }

        public Matrix Reshape(Matrix m)
        {
            return this.Reshape(m.r, m.c);
        }

        //public Matrix Reshape(Dim size)
        //{
        //    return this.Reshape(size.R, size.C);
        //}

        public Matrix Reshape(int rows, int columns)
        {
            if ((this.r * this.c) != (rows * columns))
            {
                throw new ArgumentOutOfRangeException("rows or columns", string.Concat(new object[] { "Matrix must still have same number of elements. (", this.r * this.c, ", you tried ", rows, "\x00d7", columns, " = ", rows * columns, ".)" }));
            }
            this.r = rows;
            this.c = columns;
            return this;
        }

        public Matrix ReshapeToRow()
        {
            return this.Reshape(1, this.r * this.c);
        }

        public Matrix ReshapeToCol()
        {
            return this.Reshape(this.r * this.c, 1);
        }

        public void FakeTranspose()
        {
            this.t = !this.t;
        }

        public bool IsFake()
        {
            return this.t;
        }

        public static Matrix Abs(Matrix m)
        {
            Matrix matrix = m;
            int index = 0;
            while (index < m.data.Length)
            {
                matrix.data[index] = Math.Abs(m.data[index++]);
            }
            return matrix;
        }

        public Matrix Abs()
        {
            Matrix matrix;
            this = matrix = Abs(this);
            return matrix;
        }

        public static Matrix Sign(Matrix m)
        {
            Matrix matrix = m;
            int index = 0;
            while (index < m.data.Length)
            {
                matrix.data[index] = Math.Sign(m.data[index++]);
            }
            return matrix;
        }

        public Matrix Sign()
        {
            Matrix matrix;
            this = matrix = Sign(this);
            return matrix;
        }

        public static Matrix Positive(Matrix m)
        {
            return m.MClone().Positive();
        }

        public Matrix Positive()
        {
            int length = this.data.Length;
            for (int i = 0; i < length; i++)
            {
                if (this.data[i] < 0.0)
                {
                    this.data[i] = 0.0;
                }
            }
            return this;
        }

        public static Matrix HeaviSide(Matrix m)
        {
            return m.MClone().HeaviSide();
        }

        public Matrix HeaviSide()
        {
            int length = this.data.Length;
            for (int i = 0; i < length; i++)
            {
                this.data[i] = (this.data[i] > 0.0) ? 1.0 : 0.0;
            }
            return this;
        }

        public static Matrix HeaviSide(Matrix m, double a, double fill)
        {
            return m.MClone().HeaviSide(a, fill);
        }

        public Matrix HeaviSide(double a, double fill)
        {
            int length = this.data.Length;
            for (int i = 0; i < length; i++)
            {
                this.data[i] = (this.data[i] > a) ? fill : 0.0;
            }
            return this;
        }

        public Matrix Find()
        {
            return Find(this);
        }

        public static Matrix Find(Matrix m)
        {
            return Find(m, 0.0, CmpMode.NEquals);
        }

        public static int[] FindAsArrayIdx(Matrix m)
        {
            int num = 0;
            int index = 0;
            int length = m.data.Length;
            while (index < length)
            {
                if (m.data[index] != 0.0)
                {
                    num++;
                }
                index++;
            }
            int[] numArray = new int[num];
            num = 0;
            int num4 = 0;
            int num5 = m.data.Length;
            while (num4 < num5)
            {
                if (m.data[num4] != 0.0)
                {
                    numArray[num++] = num4;
                }
                num4++;
            }
            return numArray;
        }

        public static Matrix Find(Matrix m, double d, CmpMode cmp)
        {
            int rows = 0;
            switch (cmp)
            {
                case CmpMode.Equals:
                    {
                        int index = 0;
                        int length = m.data.Length;
                        while (index < length)
                        {
                            if (m.data[index] == d)
                            {
                                rows++;
                            }
                            index++;
                        }
                        break;
                    }
                case CmpMode.NEquals:
                    {
                        int num4 = 0;
                        int num5 = m.data.Length;
                        while (num4 < num5)
                        {
                            if (m.data[num4] != d)
                            {
                                rows++;
                            }
                            num4++;
                        }
                        break;
                    }
                case CmpMode.LesThen:
                    {
                        int num6 = 0;
                        int num7 = m.data.Length;
                        while (num6 < num7)
                        {
                            if (m.data[num6] < d)
                            {
                                rows++;
                            }
                            num6++;
                        }
                        break;
                    }
                case CmpMode.GrtThen:
                    {
                        int num8 = 0;
                        int num9 = m.data.Length;
                        while (num8 < num9)
                        {
                            if (m.data[num8] > d)
                            {
                                rows++;
                            }
                            num8++;
                        }
                        break;
                    }
                case CmpMode.LesEqThen:
                    {
                        int num10 = 0;
                        int num11 = m.data.Length;
                        while (num10 < num11)
                        {
                            if (m.data[num10] <= d)
                            {
                                rows++;
                            }
                            num10++;
                        }
                        break;
                    }
                case CmpMode.GrtEqThen:
                    {
                        int num12 = 0;
                        int num13 = m.data.Length;
                        while (num12 < num13)
                        {
                            if (m.data[num12] >= d)
                            {
                                rows++;
                            }
                            num12++;
                        }
                        break;
                    }
            }
            Matrix matrix = new Matrix(rows, 2);
            rows = 0;
            switch (cmp)
            {
                case CmpMode.Equals:
                    {
                        int num14 = 0;
                        int num15 = m.data.Length;
                        while (num14 < num15)
                        {
                            if (m.data[num14] == d)
                            {
                                matrix.data[rows++] = num14 / m.c;
                                matrix.data[rows++] = num14 % m.c;
                            }
                            num14++;
                        }
                        return m;
                    }
                case CmpMode.NEquals:
                    {
                        int num16 = 0;
                        int num17 = m.data.Length;
                        while (num16 < num17)
                        {
                            if (m.data[num16] != d)
                            {
                                matrix.data[rows++] = num16 / m.c;
                                matrix.data[rows++] = num16 % m.c;
                            }
                            num16++;
                        }
                        return m;
                    }
                case CmpMode.LesThen:
                    {
                        int num18 = 0;
                        int num19 = m.data.Length;
                        while (num18 < num19)
                        {
                            if (m.data[num18] < d)
                            {
                                matrix.data[rows++] = num18 / m.c;
                                matrix.data[rows++] = num18 % m.c;
                            }
                            num18++;
                        }
                        return m;
                    }
                case CmpMode.GrtThen:
                    {
                        int num20 = 0;
                        int num21 = m.data.Length;
                        while (num20 < num21)
                        {
                            if (m.data[num20] > d)
                            {
                                matrix.data[rows++] = num20 / m.c;
                                matrix.data[rows++] = num20 % m.c;
                            }
                            num20++;
                        }
                        return m;
                    }
                case CmpMode.LesEqThen:
                    {
                        int num22 = 0;
                        int num23 = m.data.Length;
                        while (num22 < num23)
                        {
                            if (m.data[num22] <= d)
                            {
                                matrix.data[rows++] = num22 / m.c;
                                matrix.data[rows++] = num22 % m.c;
                            }
                            num22++;
                        }
                        return m;
                    }
                case CmpMode.GrtEqThen:
                    {
                        int num24 = 0;
                        int num25 = m.data.Length;
                        while (num24 < num25)
                        {
                            if (m.data[num24] >= d)
                            {
                                matrix.data[rows++] = num24 / m.c;
                                matrix.data[rows++] = num24 % m.c;
                            }
                            num24++;
                        }
                        return m;
                    }
            }
            return m;
        }

        public static int[] FindAsArrayIdx(Matrix m, double d, CmpMode cmp)
        {
            int num = 0;
            switch (cmp)
            {
                case CmpMode.Equals:
                    {
                        int index = 0;
                        int length = m.data.Length;
                        while (index < length)
                        {
                            if (m.data[index] == d)
                            {
                                num++;
                            }
                            index++;
                        }
                        break;
                    }
                case CmpMode.NEquals:
                    {
                        int num4 = 0;
                        int num5 = m.data.Length;
                        while (num4 < num5)
                        {
                            if (m.data[num4] != d)
                            {
                                num++;
                            }
                            num4++;
                        }
                        break;
                    }
                case CmpMode.LesThen:
                    {
                        int num6 = 0;
                        int num7 = m.data.Length;
                        while (num6 < num7)
                        {
                            if (m.data[num6] < d)
                            {
                                num++;
                            }
                            num6++;
                        }
                        break;
                    }
                case CmpMode.GrtThen:
                    {
                        int num8 = 0;
                        int num9 = m.data.Length;
                        while (num8 < num9)
                        {
                            if (m.data[num8] > d)
                            {
                                num++;
                            }
                            num8++;
                        }
                        break;
                    }
                case CmpMode.LesEqThen:
                    {
                        int num10 = 0;
                        int num11 = m.data.Length;
                        while (num10 < num11)
                        {
                            if (m.data[num10] <= d)
                            {
                                num++;
                            }
                            num10++;
                        }
                        break;
                    }
                case CmpMode.GrtEqThen:
                    {
                        int num12 = 0;
                        int num13 = m.data.Length;
                        while (num12 < num13)
                        {
                            if (m.data[num12] >= d)
                            {
                                num++;
                            }
                            num12++;
                        }
                        break;
                    }
            }
            int[] numArray = new int[num];
            num = 0;
            switch (cmp)
            {
                case CmpMode.Equals:
                    {
                        int num14 = 0;
                        int num15 = m.data.Length;
                        while (num14 < num15)
                        {
                            if (m.data[num14] == d)
                            {
                                numArray[num++] = num14;
                            }
                            num14++;
                        }
                        return numArray;
                    }
                case CmpMode.NEquals:
                    {
                        int num16 = 0;
                        int num17 = m.data.Length;
                        while (num16 < num17)
                        {
                            if (m.data[num16] != d)
                            {
                                numArray[num++] = num16;
                            }
                            num16++;
                        }
                        return numArray;
                    }
                case CmpMode.LesThen:
                    {
                        int num18 = 0;
                        int num19 = m.data.Length;
                        while (num18 < num19)
                        {
                            if (m.data[num18] < d)
                            {
                                numArray[num++] = num18;
                            }
                            num18++;
                        }
                        return numArray;
                    }
                case CmpMode.GrtThen:
                    {
                        int num20 = 0;
                        int num21 = m.data.Length;
                        while (num20 < num21)
                        {
                            if (m.data[num20] > d)
                            {
                                numArray[num++] = num20;
                            }
                            num20++;
                        }
                        return numArray;
                    }
                case CmpMode.LesEqThen:
                    {
                        int num22 = 0;
                        int num23 = m.data.Length;
                        while (num22 < num23)
                        {
                            if (m.data[num22] <= d)
                            {
                                numArray[num++] = num22;
                            }
                            num22++;
                        }
                        return numArray;
                    }
                case CmpMode.GrtEqThen:
                    {
                        int num24 = 0;
                        int num25 = m.data.Length;
                        while (num24 < num25)
                        {
                            if (m.data[num24] >= d)
                            {
                                numArray[num++] = num24;
                            }
                            num24++;
                        }
                        return numArray;
                    }
            }
            return numArray;
        }

        public static Matrix Find(Matrix m, double GrtEqThan, double LesEqThan)
        {
            int rows = 0;
            int index = 0;
            int length = m.data.Length;
            while (index < length)
            {
                if ((m.data[index] >= GrtEqThan) && (m.data[index] <= LesEqThan))
                {
                    rows++;
                }
                index++;
            }
            Matrix matrix = new Matrix(rows, 2);
            rows = 0;
            int num4 = 0;
            int num5 = m.data.Length;
            while (num4 < num5)
            {
                if ((m.data[num4] >= GrtEqThan) && (m.data[num4] <= LesEqThan))
                {
                    matrix.data[rows++] = num4 / m.c;
                    matrix.data[rows++] = num4 % m.c;
                }
                num4++;
            }
            return m;
        }

        public static int[] FindAsArrayIdx(Matrix m, double GrtEqThan, double LesEqThan)
        {
            int num = 0;
            int index = 0;
            int length = m.data.Length;
            while (index < length)
            {
                if ((m.data[index] >= GrtEqThan) && (m.data[index] <= LesEqThan))
                {
                    num++;
                }
                index++;
            }
            int[] numArray = new int[num];
            num = 0;
            int num4 = 0;
            int num5 = m.data.Length;
            while (num4 < num5)
            {
                if ((m.data[num4] >= GrtEqThan) && (m.data[num4] <= LesEqThan))
                {
                    numArray[num++] = num4;
                }
                num4++;
            }
            return numArray;
        }

        public static Matrix Exp(Matrix m)
        {
            return m.MClone().Exp();
        }

        public Matrix Exp()
        {
            int index = 0;
            while (index < this.data.Length)
            {
                this.data[index] = Math.Exp(this.data[index++]);
            }
            return this;
        }

        public static Matrix Sqrt(Matrix m)
        {
            Matrix matrix = m;
            int index = 0;
            while (index < m.data.Length)
            {
                matrix.data[index] = Math.Sqrt(m.data[index++]);
            }
            return matrix;
        }

        public static Matrix Pow(Matrix m, double pow)
        {
            Matrix matrix = m;
            int index = 0;
            while (index < m.data.Length)
            {
                matrix.data[index] = Math.Pow(m.data[index++], pow);
            }
            return matrix;
        }

        public Matrix Tanh()
        {
            int length = this.data.Length;
            int index = 0;
            while (index < length)
            {
                double num = Math.Exp(2.0 * this.data[index]);
                this.data[index++] = (num - 1.0) / (num + 1.0);
            }
            return this;
        }

        public static Matrix Tanh(Matrix m)
        {
            return m.MClone().Tanh();
        }

        public Matrix Cosh()
        {
            int length = this.data.Length;
            int index = 0;
            while (index < length)
            {
                this.data[index++] = (Math.Exp(this.data[index]) + Math.Exp(-this.data[index])) / 2.0;
            }
            return this;
        }

        public static Matrix Cosh(Matrix m)
        {
            return m.MClone().Cosh();
        }

        public Matrix Sech()
        {
            int length = this.data.Length;
            int index = 0;
            while (index < length)
            {
                this.data[index++] = 2.0 / (Math.Exp(this.data[index]) + Math.Exp(-this.data[index]));
            }
            return this;
        }

        public static Matrix Sech(Matrix m)
        {
            return m.MClone().Sech();
        }

        public double Max()
        {
            double minValue = double.MinValue;
            for (int i = 0; i < this.data.Length; i++)
            {
                if (this.data[i] > minValue)
                {
                    minValue = this.data[i];
                }
            }
            return minValue;
        }

        public double Min()
        {
            double maxValue = double.MaxValue;
            for (int i = 0; i < this.data.Length; i++)
            {
                if (this.data[i] < maxValue)
                {
                    maxValue = this.data[i];
                }
            }
            return maxValue;
        }

        public void Bounds(out double[] min, out double[] max)
        {
            min = new double[this.c];
            max = new double[this.c];
            for (int i = 0; i < this.c; i++)
            {
                min[i] = double.MaxValue;
                max[i] = double.MinValue;
            }
            int index = 0;
            for (int j = 0; index < this.data.Length; j = ++index % this.c)
            {
                if (this.data[index] < min[j])
                {
                    min[j] = this.data[index];
                }
                if (this.data[index] > max[j])
                {
                    max[j] = this.data[index];
                }
            }
        }

        public static void Bounds(Matrix[] matrix, out double[] min, out double[] max)
        {
            int c = matrix[0].c;
            min = new double[c];
            max = new double[c];
            for (int i = 0; i < c; i++)
            {
                min[i] = double.MaxValue;
                max[i] = double.MinValue;
            }
            foreach (Matrix matrix2 in matrix)
            {
                double[] numArray;
                double[] numArray2;
                matrix2.Bounds(out numArray, out numArray2);
                for (int j = 0; j < c; j++)
                {
                    if (numArray[j] < min[j])
                    {
                        min[j] = numArray[j];
                    }
                    if (numArray2[j] > max[j])
                    {
                        max[j] = numArray2[j];
                    }
                }
            }
        }

        public void Bounds(out double min, out double max)
        {
            min = double.MaxValue;
            max = double.MinValue;
            for (int i = 0; i < this.data.Length; i++)
            {
                if (this.data[i] < min)
                {
                    min = this.data[i];
                }
                if (this.data[i] > max)
                {
                    max = this.data[i];
                }
            }
        }

        public static void Bounds(Matrix[] matrix, out double min, out double max)
        {
            min = double.MaxValue;
            max = double.MinValue;
            foreach (Matrix matrix2 in matrix)
            {
                double num;
                double num2;
                matrix2.Bounds(out num, out num2);
                if (num < min)
                {
                    min = num;
                }
                if (num2 > max)
                {
                    max = num2;
                }
            }
        }

        public static Matrix Concat(bool Columns, params Matrix[] m)
        {
            if (m.Length < 0)
            {
                throw new ArgumentException("Please specify at least one Matrix");
            }
            if (m.Length == 1)
            {
                return m[0].MClone();
            }
            Matrix matrix = new Matrix(0, 0, null, false);
            if (Columns)
            {
                matrix.r = 0x7fffffff;
                int r = 0x7fffffff;
                for (int j = 0; j < m.Length; j++)
                {
                    matrix.c += m[j].c;
                    if ((m[j].r < matrix.r) && (m[j].r > 1))
                    {
                        matrix.r = m[j].r;
                    }
                    if ((m[j].r < r) && (m[j].r > 0))
                    {
                        r = m[j].r;
                    }
                }
                if (matrix.r == 0x7fffffff)
                {
                    if (r == 0x7fffffff)
                    {
                        throw new ArgumentException("No matrix has any rows!");
                    }
                    matrix.r = r;
                }
                matrix.data = new double[matrix.r * matrix.c];
                int num3 = 0;
                int destinationIndex = 0;
                while (num3 < matrix.r)
                {
                    int num5 = 0;
                    while (num5 < m.Length)
                    {
                        Array.Copy(m[num5].data, (m[num5].r > 1) ? (num3 * m[num5].c) : 0, matrix.data, destinationIndex, m[num5].c);
                        destinationIndex += m[num5++].c;
                    }
                    num3++;
                }
                return matrix;
            }
            matrix.c = 0x7fffffff;
            for (int i = 0; i < m.Length; i++)
            {
                matrix.r += m[i].r;
                if (m[i].c < matrix.c)
                {
                    matrix.c = m[i].c;
                }
            }
            matrix.data = new double[matrix.r * matrix.c];
            int index = 0;
            int num8 = 0;
            while (index < m.Length)
            {
                if (m[index].c != matrix.c)
                {
                    int num9 = 0;
                    while (num9 < m[index].r)
                    {
                        Array.Copy(m[num8].data, num9 * m[index].c, matrix.data, num8 * matrix.c, matrix.c);
                        num9++;
                        num8++;
                    }
                }
                else
                {
                    m[index].data.CopyTo(matrix.data, (int)(num8 * matrix.c));
                    num8 += m[index].r;
                }
                index++;
            }
            return matrix;
        }

        public Matrix Concat(bool Columns, Matrix m)
        {
            Matrix matrix;
            Matrix[] matrixArray = new Matrix[] { this, m };
            this = matrix = Concat(Columns, matrixArray);
            return matrix;
        }

        public static Matrix Tile(Matrix m, int r, int c)
        {
            if (r < 1)
            {
                throw new ArgumentOutOfRangeException("r=" + r, "r must be  > 0.");
            }
            if (c < 1)
            {
                throw new ArgumentOutOfRangeException("c=" + c, "c must be  > 0.");
            }
            double[] array = new double[((m.r * r) * m.c) * c];
            if ((m.r == 1) && (c == 1))
            {
                for (int j = 0; j < r; j++)
                {
                    m.data.CopyTo(array, (int)(j * m.data.Length));
                }
                return new Matrix(r, m.c, array, false);
            }
            for (int i = 0; i < r; i++)
            {
                for (int k = 0; k < m.r; k++)
                {
                    for (int n = 0; n < c; n++)
                    {
                        Array.Copy(m.data, k * m.c, array, (((i * m.data.Length) * c) + ((k * m.c) * c)) + (n * m.c), m.c);
                    }
                }
            }
            return new Matrix(m.r * r, m.c * c, array, false);
        }

        public static Matrix Eye(int cols)
        {
            Matrix matrix = new Matrix(cols, cols);
            for (int i = 0; i < matrix.c; i++)
            {
                matrix.data[(i * matrix.c) + i] = 1.0;
            }
            return matrix;
        }

        public static Matrix Diagonal(params double[] a)
        {
            Matrix matrix = new Matrix(a.Length, a.Length);
            for (int i = 0; i < matrix.c; i++)
            {
                matrix.data[(i * matrix.c) + i] = a[i];
            }
            return matrix;
        }

        public static Matrix Diagonal(double a, int size)
        {
            Matrix matrix = new Matrix(size, size);
            for (int i = 0; i < matrix.c; i++)
            {
                matrix.data[(i * matrix.c) + i] = a;
            }
            return matrix;
        }

        public Matrix Diagonal()
        {
            Matrix matrix;
            if ((this.r == 0) || (this.c == 0))
            {
                throw new ArgumentException("Error: Empty Matrix.");
            }
            if ((this.r == 1) || (this.c == 1))
            {
                matrix = new Matrix(this.data.Length, this.data.Length);
                for (int j = 0; j < matrix.c; j++)
                {
                    matrix.data[(j * matrix.c) + j] = this.data[j];
                }
                return matrix;
            }
            int rows = Math.Min(this.r, this.c);
            matrix = new Matrix(rows, rows);
            for (int i = 0; i < rows; i++)
            {
                matrix.data[(i * rows) + i] = this.data[(i * this.c) + i];
            }
            return matrix;
        }

        public Matrix DiagAsRow()
        {
            Matrix matrix = new Matrix(1, Math.Min(this.r, this.c));
            for (int i = 0; i < matrix.c; i++)
            {
                matrix.data[i] = this.data[(i * this.c) + i];
            }
            return matrix;
        }

        public Matrix[] Split(bool columns, int parts)
        {
            Matrix[] matrixArray;
            if (columns)
            {
                if ((this.Columns % parts) != 0)
                {
                    throw new ArgumentException(string.Concat(new object[] { "The ", this.c, " columns can not be equaly divided into ", parts, " parts." }));
                }
                matrixArray = new Matrix[parts];
                int num = this.c / parts;
                for (int j = 0; j < parts; j++)
                {
                    matrixArray[j] = this.MultipleColumns(j * num, num);
                }
                return matrixArray;
            }
            if ((this.Columns % parts) != 0)
            {
                throw new ArgumentException(string.Concat(new object[] { "The ", this.c, " columns can not be equaly divided into ", parts, " parts." }));
            }
            matrixArray = new Matrix[parts];
            int length = (this.r / parts) * this.c;
            for (int i = 0; i < parts; i++)
            {
                double[] destinationArray = new double[length];
                Array.Copy(this.data, i * length, destinationArray, 0, length);
                matrixArray[i] = new Matrix(length / this.c, this.c, destinationArray, false);
            }
            return matrixArray;
        }

        public Matrix Limit(double min, double max, double NaNsubst)
        {
            if (min > max)
            {
                throw new ArgumentException("min cannot be greater than max.", "min, max");
            }
            for (int i = 0; i < this.data.Length; i++)
            {
                if (this.data[i] < min)
                {
                    this.data[i] = min;
                }
                else if (this.data[i] > max)
                {
                    this.data[i] = max;
                }
                else if (double.IsNaN(this.data[i]))
                {
                    this.data[i] = NaNsubst;
                }
            }
            return this;
        }

        public Matrix Limit(double min, double max)
        {
            if (min > max)
            {
                throw new ArgumentException("min cannot be greater than max.", "min, max");
            }
            for (int i = 0; i < this.data.Length; i++)
            {
                if (this.data[i] < min)
                {
                    this.data[i] = min;
                }
                else if (this.data[i] > max)
                {
                    this.data[i] = max;
                }
            }
            return this;
        }

        public Matrix Shuffle(bool rows)
        {
            if (rows)
            {
                double[] destinationArray = new double[this.c];
                for (int i = 0; i < this.r; i++)
                {
                    int sourceIndex = this.c * (i + (rand.Next(this.r) % (this.r - i)));
                    Array.Copy(this.data, sourceIndex, destinationArray, 0, this.c);
                    Array.Copy(this.data, i * this.c, this.data, sourceIndex, this.c);
                    Array.Copy(destinationArray, 0, this.data, i * this.c, this.c);
                }
            }
            else
            {
                for (int j = 0; j < this.c; j++)
                {
                    int num4 = j + (rand.Next(this.c) % (this.c - j));
                    for (int k = 0; k < this.r; k++)
                    {
                        double num6 = this.data[(this.r * this.c) + num4];
                        this.data[(this.r * this.c) + num4] = this.data[(this.r * this.c) + j];
                        this.data[(this.r * this.c) + j] = num6;
                    }
                }
            }
            return this;
        }

        public Matrix Shuffle(bool rows, int seed)
        {
            Random random = new Random(seed);
            if (rows)
            {
                double[] destinationArray = new double[this.c];
                for (int i = 0; i < this.r; i++)
                {
                    int sourceIndex = this.c * (i + (random.Next(this.r) % (this.r - i)));
                    Array.Copy(this.data, sourceIndex, destinationArray, 0, this.c);
                    Array.Copy(this.data, i * this.c, this.data, sourceIndex, this.c);
                    Array.Copy(destinationArray, 0, this.data, i * this.c, this.c);
                }
            }
            else
            {
                for (int j = 0; j < this.c; j++)
                {
                    int num4 = j + (random.Next(this.c) % (this.c - j));
                    for (int k = 0; k < this.r; k++)
                    {
                        double num6 = this.data[(this.r * this.c) + num4];
                        this.data[(this.r * this.c) + num4] = this.data[(this.r * this.c) + j];
                        this.data[(this.r * this.c) + j] = num6;
                    }
                }
            }
            return this;
        }

        public static void Scale(Matrix m, double a, double b, double min, double max)
        {
            if (a == b)
            {
                m.Fill((max - min) / 2.0);
            }
            else
            {
                double num = (max - min) / (b - a);
                int length = m.data.Length;
                int index = 0;
                while (index < length)
                {
                    m.data[index] = ((m.data[index++] - a) * num) + min;
                }
            }
        }

        //public static Matrix Scaled(Matrix m, ScaleInfo[] info)
        //{
        //    Matrix matrix = m.MClone();
        //    Scale(matrix, info);
        //    return matrix;
        //}

        //public static void Scale(Matrix m, ScaleInfo[] info)
        //{
        //    if (m.c != info.Length)
        //    {
        //        throw new ArgumentException("");
        //    }
        //    for (int i = 0; i < m.c; i++)
        //    {
        //        Matrix matrix = m[false, new int[] { i }];
        //        Scale(matrix, info[i]);
        //        m[false, new int[] { i }] = matrix;
        //    }
        //}

        //public static void Scale(Matrix m, ScaleInfo info)
        //{
        //    if (info.MinIn == info.MaxIn)
        //    {
        //        m.Fill(info.OutSpan / 2.0);
        //    }
        //    else
        //    {
        //        m.Minus(info.MinIn).Product(info.Scale).Plus(info.MinOut);
        //    }
        //}

        //public Matrix Scale(params ScaleInfo[] info)
        //{
        //    if (info.Length == 1)
        //    {
        //        Scale(this, info[0]);
        //    }
        //    else
        //    {
        //        Scale(this, info);
        //    }
        //    return this;
        //}

        //public Matrix Scale(double min, double max)
        //{
        //    double num;
        //    double num2;
        //    this.Bounds(out num, out num2);
        //    Scale(this, num, num2, min, max);
        //    return this;
        //}

        //public Matrix Normalize(params NormalizeInfo[] info)
        //{
        //    if (info.Length == 1)
        //    {
        //        this.Minus(info[0].Mean).DivideBy(info[0].NxStdv);
        //    }
        //    else
        //    {
        //        this.Minus(NormalizeInfo.MeanMatrix(info)).DivideBy(NormalizeInfo.NxStdvMatrix(info));
        //    }
        //    return this;
        //}

        //public static Matrix Normalized(Matrix m, params NormalizeInfo[] info)
        //{
        //    if ((info.Length != m.c) && (info.Length != 1))
        //    {
        //        throw new ArgumentException("info.Length must be 1 or == m.Columns");
        //    }
        //    if (info.Length == 1)
        //    {
        //        Matrix matrix = m - ((Matrix)info[0].Mean);
        //        return matrix.DivideBy(info[0].NxStdv);
        //    }
        //    Matrix matrix2 = m - NormalizeInfo.MeanMatrix(info);
        //    return matrix2.DivideBy(NormalizeInfo.NxStdvMatrix(info));
        //}

        //public static Matrix DeNormalized(Matrix m, params NormalizeInfo[] info)
        //{
        //    if ((info.Length != m.c) && (info.Length != 1))
        //    {
        //        throw new ArgumentException("info.Length must be 1 or == m.Columns");
        //    }
        //    if (info.Length == 1)
        //    {
        //        return (Matrix)((m * info[0].NxStdv) + info[0].Mean);
        //    }
        //    return m.MClone().Pointprod(NormalizeInfo.NxStdvMatrix(info)).Plus(NormalizeInfo.MeanMatrix(info));
        //}

        public static Matrix operator +(Matrix p, Matrix q)
        {
            return p.MClone().Plus(q);
        }

        public static Matrix operator -(Matrix p, Matrix q)
        {
            return p.MClone().Minus(q);
        }

        public static Matrix operator +(Matrix m, double d)
        {
            return m.MClone().Plus(d);
        }

        public static Matrix operator +(double d, Matrix m)
        {
            return m.MClone().Plus(d);
        }

        public static Matrix operator -(Matrix m, double d)
        {
            return m.MClone().Minus(d);
        }

        public static Matrix operator -(double d, Matrix m)
        {
            return m.MClone().PreMinus(d);
        }

        public static Matrix operator -(Matrix m)
        {
            double[] data = new double[m.data.Length];
            int index = 0;
            while (index < m.data.Length)
            {
                data[index] = -m.data[index++];
            }
            return new Matrix(m.r, m.c, data, m.t, m.noiseLevel);
        }

        public static Matrix operator *(Matrix p, Matrix q)
        {
            return p.Product(q);
        }

        public static Matrix operator *(Matrix m, double d)
        {
            return m.MClone().Product(d);
        }

        public static Matrix operator *(double d, Matrix m)
        {
            return m.MClone().Product(d);
        }

        public static Matrix operator /(Matrix p, Matrix q)
        {
            return p.MClone().DivideBy(q);
        }

        public static Matrix operator /(Matrix m, double d)
        {
            return m.MClone().DivideBy(d);
        }

        public static Matrix operator /(double d, Matrix m)
        {
            Matrix matrix = m.MClone();
            int index = 0;
            while (index < matrix.data.Length)
            {
                matrix.data[index] = d / matrix.data[index++];
            }
            return matrix;
        }

        public static Matrix operator &(Matrix p, Matrix q)
        {
            Matrix[] m = new Matrix[] { p, q };
            return Concat(true, m);
        }

        public static Matrix operator |(Matrix p, Matrix q)
        {
            Matrix[] m = new Matrix[] { p, q };
            return Concat(false, m);
        }

        public static bool operator ==(Matrix p, Matrix q)
        {
            if ((p.c != q.c) || (p.r != q.r))
            {
                return false;
            }
            if (p.data.Length != q.data.Length)
            {
                return false;
            }
            for (int i = 0; i < p.data.Length; i++)
            {
                if (p.data[i] != q.Data[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static bool operator !=(Matrix p, Matrix q)
        {
            return !(p == q);
        }

        public static bool SameSize(Matrix m1, Matrix m2)
        {
            return ((m1.r == m2.r) && (m1.c == m2.c));
        }

        public bool SameSize(Matrix m)
        {
            return ((this.r == m.r) && (this.c == m.c));
        }

        public object Clone()
        {
            return this.MClone();
        }

        public Matrix MClone()
        {
            double[] array = new double[this.data.Length];
            this.data.CopyTo(array, 0);
            return new Matrix(this.r, this.c, array, this.t, this.noiseLevel);
        }

        public bool IsEmpty()
        {
            return (this.data == null);
        }

        public override bool Equals(object obj)
        {
            return ((obj is Matrix) && this.Equals((Matrix)obj));
        }

        public bool Equals(Matrix m)
        {
            return (this == m);
        }

        public bool Equals(Matrix m, double e)
        {
            if (((this.r != m.r) || (this.c != m.c)) || (this.data.Length != m.data.Length))
            {
                return false;
            }
            Matrix matrix = this - m;
            return (matrix.Abs().Sum(true).Sum(false)[0] < (e * this.data.Length));
        }

        public double NoiseLevel
        {
            get
            {
                return this.noiseLevel;
            }
            set
            {
                this.noiseLevel = value;
            }
        }
        public object SyncRoot
        {
            get
            {
                return this.data.SyncRoot;
            }
        }
        public string ToTabedString()
        {
            string str = string.Concat(new object[] { "Matrix has ", this.Rows, " rows and ", this.Columns, " Columns\n" });
            if ((this.Columns < 0x19) && (this.Rows < 50))
            {
                for (int i = 0; i < this.Rows; i++)
                {
                    for (int j = 0; j < this.Columns; j++)
                    {
                        str = str + ((float)this[i, j]) + "\t";
                    }
                    str = str + "\n";
                }
                return str;
            }
            return (str + "Matrix is believed to be large.");
        }

        public string ToTabedString(int row, int col, int endRow, int endCol)
        {
            return "";
        }

        public string ToBase64String()
        {
            return Convert.ToBase64String(this.ToByteArray());
        }

        public byte[] ToByteArray()
        {
            byte[] dst = new byte[Buffer.ByteLength(this.data)];
            Buffer.BlockCopy(this.data, 0, dst, 0, dst.Length);
            return dst;
        }

        public static string ToCSV(Matrix m)
        {
            StringBuilder builder = new StringBuilder();
            int index = 0;
            while (index < m.data.Length)
            {
                builder.Append(m.data[index]);
                builder.Append(", ");
                if ((++index % m.c) == 0)
                {
                    builder.Append('\n');
                }
            }
            return builder.ToString();
        }

        public bool HasNan()
        {
            for (int i = 0; i < this.data.Length; i++)
            {
                if (double.IsNaN(this.data[i]))
                {
                    return true;
                }
            }
            return false;
        }

        public bool HasInfinity()
        {
            for (int i = 0; i < this.data.Length; i++)
            {
                if (double.IsInfinity(this.data[i]))
                {
                    return true;
                }
            }
            return false;
        }

        public bool HasPositiveInfinity()
        {
            for (int i = 0; i < this.data.Length; i++)
            {
                if (double.IsPositiveInfinity(this.data[i]))
                {
                    return true;
                }
            }
            return false;
        }

        public bool HasNegativeInfinity()
        {
            for (int i = 0; i < this.data.Length; i++)
            {
                if (double.IsNegativeInfinity(this.data[i]))
                {
                    return true;
                }
            }
            return false;
        }

        public static Matrix[] Clone(Matrix[] m)
        {
            Matrix[] matrixArray = new Matrix[m.Length];
            for (int i = 0; i < m.Length; i++)
            {
                matrixArray[i] = m[i].MClone();
            }
            return matrixArray;
        }

        internal void FlipRows()
        {
            double[] sourceArray = (double[])this.data.Clone();
            int num = 0;
            while (num < this.r)
            {
                Array.Copy(sourceArray, num * this.c, this.data, (this.r - ++num) * this.c, this.c);
            }
        }

        public Matrix Inverse()
        {
            return this.Inverse(false);
        }

        public Matrix Inverse(bool pseudoInverse)
        {
            if (!pseudoInverse)
            {
                if (this.c != this.r)
                {
                    throw new ArgumentException(string.Concat(new object[] { "??? Error using ==> *\nMatrix  must be square.\nYou tried INV[(", this.r, "x", this.c, "])" }));
                }
                LUDecomposition decomposition = new LUDecomposition(this);
                return decomposition.Solve(Eye(this.r));
            }
            QRDecomposition decomposition2 = new QRDecomposition(this);
            return decomposition2.Solve(Eye(this.r));
        }

        static Matrix()
        {
            rand = new Random();
            End = -2147483648;
            rgen = new Random();
        }
        // Nested Types
        public enum CmpMode
        {
            Equals,
            NEquals,
            LesThen,
            GrtThen,
            LesEqThen,
            GrtEqThen
        }
    }
}
