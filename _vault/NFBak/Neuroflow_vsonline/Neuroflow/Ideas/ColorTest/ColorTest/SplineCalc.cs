using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Diagnostics.Contracts;

namespace ColorTest
{
    public sealed class SplineCalc
    {
        public SplineCalc(Point[] points)
        {
            Contract.Requires(points != null);
            Contract.Requires(points.Length > 1);

            this.points = points;
        }

        Point[] points;

        public int GetY(int x)
        {
            for (int i = 0; i < points.Length - 1; i++)
            {
                var cp = points[i];
                var np = points[i + 1];
                if (cp.X <= x && x <= np.X) 
                {
                    return GetY(x, cp, np);
                }
            }
            return GetY(x, points[points.Length - 2], points[points.Length - 1]);
        }

        private int GetY(int x, Point cp, Point np)
        {
            double mu = (double)(x - cp.X) / (double)(np.X - cp.X);
            double mu2 = (1.0 - Math.Cos(mu * Math.PI)) / 2.0;
            double y = ((double)cp.Y * (1 - mu2) + (double)np.Y * mu2);
            return (int)Math.Round(y, MidpointRounding.AwayFromZero);
        }

        public int GetX(int y)
        {
            throw new NotSupportedException();
        }


        /*public SplineCalc(Point[] points)
        {
            var last = points[points.Length - 1];
            values = new int[last.X + 1];
            var sd = SecondDerivative(points);

            for (int i = 0; i < points.Length - 1; i++)
            {
                Point cur = points[i];
                Point next = points[i + 1];

                for (int x = cur.X; x <= next.X; x++)
                {
                    double t = (double)(x - cur.X) / (next.X - cur.X);

                    double a = 1 - t;
                    double b = t;
                    double h = next.X - cur.X;

                    double y = a * cur.Y + b * next.Y + (h * h / 6) * ((a * a * a - a) * sd[i] + (b * b * b - b) * sd[i + 1]);

                    values[x] = Math.Min(Math.Max((int)Math.Round(y, MidpointRounding.AwayFromZero), 0), values.Length - 1);
                }
            }
        }

        int[] values;

        public int GetY(int x)
        {
            Contract.Requires(x >= 0 && x < values.Length);

            return values[x];
        }

        public int GetX(int y)
        {
            if (values[0] > y) return 0;
            for (int xCurr = 0; xCurr < values.Length - 1; xCurr++)
            {
                int xNext = xCurr + 1;
                int yCurr = values[xCurr];
                int yNext = values[xNext];

                if (y >= yCurr && y < yNext) return xCurr;
            }
            return values.Length - 1;
        }

        static double[] SecondDerivative(Point[] P)
        {
            int n = P.Length;

            // build the tridiagonal system 
            // (assume 0 boundary conditions: y2[0]=y2[-1]=0) 
            double[,] matrix = new double[n, 3];
            double[] result = new double[n];
            matrix[0, 1] = 1;
            for (int i = 1; i < n - 1; i++)
            {
                matrix[i, 0] = (double)(P[i].X - P[i - 1].X) / 6;
                matrix[i, 1] = (double)(P[i + 1].X - P[i - 1].X) / 3;
                matrix[i, 2] = (double)(P[i + 1].X - P[i].X) / 6;
                result[i] = (double)(P[i + 1].Y - P[i].Y) / (P[i + 1].X - P[i].X) - (double)(P[i].Y - P[i - 1].Y) / (P[i].X - P[i - 1].X);
            }
            matrix[n - 1, 1] = 1;

            // solving pass1 (up->down)
            for (int i = 1; i < n; i++)
            {
                double k = matrix[i, 0] / matrix[i - 1, 1];
                matrix[i, 1] -= k * matrix[i - 1, 2];
                matrix[i, 0] = 0;
                result[i] -= k * result[i - 1];
            }
            // solving pass2 (down->up)
            for (int i = n - 2; i >= 0; i--)
            {
                double k = matrix[i, 2] / matrix[i + 1, 1];
                matrix[i, 1] -= k * matrix[i + 1, 0];
                matrix[i, 2] = 0;
                result[i] -= k * result[i + 1];
            }

            // return second derivative value for each point P
            double[] y2 = new double[n];
            for (int i = 0; i < n; i++) y2[i] = result[i] / matrix[i, 1];
            return y2;
        }*/
    }
}
