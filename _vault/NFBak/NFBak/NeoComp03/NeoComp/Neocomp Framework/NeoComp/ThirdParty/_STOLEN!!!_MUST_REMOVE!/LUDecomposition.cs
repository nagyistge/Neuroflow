using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThirdParty
{
    public class LUDecomposition
    {
        // Fields
        private double[][] LU;
        private int m;
        private int n;
        private int[] piv;
        private int pivsign;

        // Methods
        public LUDecomposition(Matrix A)
        {
            this.LU = A.ToJaggedArray();
            this.m = A.Rows;
            this.n = A.Columns;
            this.piv = new int[this.m];
            for (int i = 0; i < this.m; i++)
            {
                this.piv[i] = i;
            }
            this.pivsign = 1;
            double[] numArray2 = new double[this.m];
            for (int j = 0; j < this.n; j++)
            {
                for (int k = 0; k < this.m; k++)
                {
                    numArray2[k] = this.LU[k][j];
                }
                for (int m = 0; m < this.m; m++)
                {
                    double[] numArray = this.LU[m];
                    int num5 = Math.Min(m, j);
                    double num6 = 0.0;
                    for (int num7 = 0; num7 < num5; num7++)
                    {
                        num6 += numArray[num7] * numArray2[num7];
                    }
                    numArray[j] = numArray2[m] -= num6;
                }
                int index = j;
                for (int n = j + 1; n < this.m; n++)
                {
                    if (Math.Abs(numArray2[n]) > Math.Abs(numArray2[index]))
                    {
                        index = n;
                    }
                }
                if (index != j)
                {
                    for (int num10 = 0; num10 < this.n; num10++)
                    {
                        double num11 = this.LU[index][num10];
                        this.LU[index][num10] = this.LU[j][num10];
                        this.LU[j][num10] = num11;
                    }
                    int num12 = this.piv[index];
                    this.piv[index] = this.piv[j];
                    this.piv[j] = num12;
                    this.pivsign = -this.pivsign;
                }
                if ((j < this.m) & (this.LU[j][j] != 0.0))
                {
                    for (int num13 = j + 1; num13 < this.m; num13++)
                    {
                        this.LU[num13][j] /= this.LU[j][j];
                    }
                }
            }
        }

        public virtual double Determinant()
        {
            if (this.m != this.n)
            {
                throw new ArgumentException("Matrix must be square.");
            }
            double pivsign = this.pivsign;
            for (int i = 0; i < this.n; i++)
            {
                pivsign *= this.LU[i][i];
            }
            return pivsign;
        }

        public virtual Matrix Solve(Matrix B)
        {
            if (B.Rows != this.m)
            {
                throw new ArgumentException("Matrix row dimensions must agree.");
            }
            if (!this.IsNonSingular)
            {
                throw new SystemException("matrix is singular.");
            }
            int columns = B.Columns;
            Matrix matrix = B[true, this.piv];
            double[][] array = matrix.ToJaggedArray();
            for (int i = 0; i < this.n; i++)
            {
                for (int k = i + 1; k < this.n; k++)
                {
                    for (int m = 0; m < columns; m++)
                    {
                        array[k][m] -= array[i][m] * this.LU[k][i];
                    }
                }
            }
            for (int j = this.n - 1; j >= 0; j--)
            {
                for (int n = 0; n < columns; n++)
                {
                    array[j][n] /= this.LU[j][j];
                }
                for (int num7 = 0; num7 < j; num7++)
                {
                    for (int num8 = 0; num8 < columns; num8++)
                    {
                        array[num7][num8] -= array[j][num8] * this.LU[num7][j];
                    }
                }
            }
            matrix.FromJaggedArray(array);
            return matrix;
        }

        // Properties
        public virtual double[] DoublePivot
        {
            get
            {
                double[] numArray = new double[this.m];
                for (int i = 0; i < this.m; i++)
                {
                    numArray[i] = this.piv[i];
                }
                return numArray;
            }
        }

        public virtual bool IsNonSingular
        {
            get
            {
                for (int i = 0; i < this.n; i++)
                {
                    if (this.LU[i][i] == 0.0)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public virtual Matrix L
        {
            get
            {
                Matrix matrix = new Matrix(this.m, this.n);
                double[][] array = matrix.ToJaggedArray();
                for (int i = 0; i < this.m; i++)
                {
                    for (int j = 0; j < this.n; j++)
                    {
                        if (i > j)
                        {
                            array[i][j] = this.LU[i][j];
                        }
                        else if (i == j)
                        {
                            array[i][j] = 1.0;
                        }
                        else
                        {
                            array[i][j] = 0.0;
                        }
                    }
                }
                matrix.FromJaggedArray(array);
                return matrix;
            }
        }

        public virtual int[] Pivot
        {
            get
            {
                int[] numArray = new int[this.m];
                for (int i = 0; i < this.m; i++)
                {
                    numArray[i] = this.piv[i];
                }
                return numArray;
            }
        }

        public virtual Matrix U
        {
            get
            {
                Matrix matrix = new Matrix(this.n, this.n);
                double[][] array = matrix.ToJaggedArray();
                for (int i = 0; i < this.n; i++)
                {
                    for (int j = 0; j < this.n; j++)
                    {
                        if (i <= j)
                        {
                            array[i][j] = this.LU[i][j];
                        }
                        else
                        {
                            array[i][j] = 0.0;
                        }
                    }
                }
                matrix.FromJaggedArray(array);
                return matrix;
            }
        }
    }
}
