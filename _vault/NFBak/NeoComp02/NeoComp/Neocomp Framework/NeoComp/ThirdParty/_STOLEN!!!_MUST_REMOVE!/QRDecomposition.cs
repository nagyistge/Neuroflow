using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThirdParty
{
    public class QRDecomposition
    {
        // Fields
        private int m;
        private int n;
        private double[][] QR;
        private double[] Rdiag;

        // Methods
        public QRDecomposition(Matrix A)
        {
            this.QR = A.ToJaggedArray();
            this.m = A.Rows;
            this.n = A.Columns;
            this.Rdiag = new double[this.n];
            for (int i = 0; i < this.n; i++)
            {
                double a = 0.0;
                for (int j = i; j < this.m; j++)
                {
                    a = this.Hypot(a, this.QR[j][i]);
                }
                if (a != 0.0)
                {
                    if (this.QR[i][i] < 0.0)
                    {
                        a = -a;
                    }
                    for (int k = i; k < this.m; k++)
                    {
                        this.QR[k][i] /= a;
                    }
                    this.QR[i][i]++;
                    for (int m = i + 1; m < this.n; m++)
                    {
                        double num6 = 0.0;
                        for (int n = i; n < this.m; n++)
                        {
                            num6 += this.QR[n][i] * this.QR[n][m];
                        }
                        num6 = -num6 / this.QR[i][i];
                        for (int num8 = i; num8 < this.m; num8++)
                        {
                            this.QR[num8][m] += num6 * this.QR[num8][i];
                        }
                    }
                }
                this.Rdiag[i] = -a;
            }
        }

        public double Hypot(double a, double b)
        {
            double num;
            if (Math.Abs(a) > Math.Abs(b))
            {
                num = b / a;
                return (Math.Abs(a) * Math.Sqrt(1.0 + (num * num)));
            }
            if (b != 0.0)
            {
                num = a / b;
                return (Math.Abs(b) * Math.Sqrt(1.0 + (num * num)));
            }
            return 0.0;
        }

        public virtual Matrix Solve(Matrix B)
        {
            if (B.Rows != this.m)
            {
                throw new ArgumentException("Matrix row dimensions must agree.");
            }
            if (!this.FullRank)
            {
                throw new SystemException("Matrix is rank deficient.");
            }
            int columns = B.Columns;
            double[][] array = B.ToJaggedArray();
            for (int i = 0; i < this.n; i++)
            {
                for (int k = 0; k < columns; k++)
                {
                    double num4 = 0.0;
                    for (int m = i; m < this.m; m++)
                    {
                        num4 += this.QR[m][i] * array[m][k];
                    }
                    num4 = -num4 / this.QR[i][i];
                    for (int n = i; n < this.m; n++)
                    {
                        array[n][k] += num4 * this.QR[n][i];
                    }
                }
            }
            for (int j = this.n - 1; j >= 0; j--)
            {
                for (int num8 = 0; num8 < columns; num8++)
                {
                    array[j][num8] /= this.Rdiag[j];
                }
                for (int num9 = 0; num9 < j; num9++)
                {
                    for (int num10 = 0; num10 < columns; num10++)
                    {
                        array[num9][num10] -= array[j][num10] * this.QR[num9][j];
                    }
                }
            }
            B.FromJaggedArray(array);
            return B[0, this.n, 0, columns];
        }

        // Properties
        public virtual bool FullRank
        {
            get
            {
                for (int i = 0; i < this.n; i++)
                {
                    if (this.Rdiag[i] == 0.0)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public virtual Matrix H
        {
            get
            {
                Matrix matrix = new Matrix(this.m, this.n);
                double[][] array = matrix.ToJaggedArray();
                for (int i = 0; i < this.m; i++)
                {
                    for (int j = 0; j < this.n; j++)
                    {
                        if (i >= j)
                        {
                            array[i][j] = this.QR[i][j];
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

        public virtual Matrix Q
        {
            get
            {
                Matrix matrix = new Matrix(this.m, this.n);
                double[][] array = matrix.ToJaggedArray();
                for (int i = this.n - 1; i >= 0; i--)
                {
                    for (int j = 0; j < this.m; j++)
                    {
                        array[j][i] = 0.0;
                    }
                    array[i][i] = 1.0;
                    for (int k = i; k < this.n; k++)
                    {
                        if (this.QR[i][i] != 0.0)
                        {
                            double num4 = 0.0;
                            for (int m = i; m < this.m; m++)
                            {
                                num4 += this.QR[m][i] * array[m][k];
                            }
                            num4 = -num4 / this.QR[i][i];
                            for (int n = i; n < this.m; n++)
                            {
                                array[n][k] += num4 * this.QR[n][i];
                            }
                        }
                    }
                }
                matrix.FromJaggedArray(array);
                return matrix;
            }
        }

        public virtual Matrix R
        {
            get
            {
                Matrix matrix = new Matrix(this.n, this.n);
                double[][] array = matrix.ToJaggedArray();
                for (int i = 0; i < this.n; i++)
                {
                    for (int j = 0; j < this.n; j++)
                    {
                        if (i < j)
                        {
                            array[i][j] = this.QR[i][j];
                        }
                        else if (i == j)
                        {
                            array[i][j] = this.Rdiag[i];
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
