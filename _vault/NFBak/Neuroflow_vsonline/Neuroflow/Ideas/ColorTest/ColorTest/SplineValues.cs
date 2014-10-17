using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorTest
{
    public class SplineValues : IAdjustmentValueSource
    {
        class DistComp : IEqualityComparer<Point>
        {
            public bool Equals(Point x, Point y)
            {
                return x.X == y.X;
            }

            public int GetHashCode(Point obj)
            {
                return obj.X;
            }
        }

        public SplineValues(int pointCount, bool reversible, int max = 255)
        {
            Contract.Requires(pointCount > 0);
            Contract.Requires(max > 0);

            PointCount = pointCount;
            Reversible = reversible;
            Max = max;
        }

        public int PointCount { get; private set; }

        public bool Reversible { get; private set; }

        public int Max { get; private set; }

        public Point[] Points { get; private set; }

        SplineCalc splineCalc;

        public void Init(double[] values, ref int valueIndex)
        {
            var points = new List<Point>(PointCount);
            for (int i = 0; i < PointCount; i++)
            {
                double x, y;
                if (i == 0)
                {
                    x = 0;
                    y = (values[valueIndex++] * (double)Max * 1.5) - (double)Max / 2.0;
                }
                else if (i == PointCount - 1)
                {
                    x = Max;
                    y = (values[valueIndex++] * (double)Max * 1.5) - (double)Max / 2.0;
                }
                else
                {
                    x = values[valueIndex++] * (double)Max;
                    y = (values[valueIndex++] * (double)Max * 1.5) - (double)Max / 2.0;
                }
                var p = new Point((int)Math.Round(x, MidpointRounding.AwayFromZero), (int)Math.Round(y, MidpointRounding.AwayFromZero));
                points.Add(p);
            }

            Points = points.Distinct(new DistComp()).OrderBy(p => p.X).ToArray();

            if (Reversible) SortY();

            splineCalc = new SplineCalc(Points);
        }

        public double GetY(double x, double max)
        {
            VerifyInit();
            x = Transform(x, max);
            x = Math.Min(Math.Max(0, x), Max);
            return TransformBack(splineCalc.GetY((int)Math.Round(x, MidpointRounding.AwayFromZero)), max);
        }

        public double GetX(double y, double max)
        {
            VerifyInit();
            if (!Reversible) throw new InvalidOperationException("SplineValues is not reversible.");
            y = Transform(y, max);
            y = Math.Min(Math.Max(0, y), Max);
            return TransformBack(splineCalc.GetX((int)Math.Round(y, MidpointRounding.AwayFromZero)), max);
        }

        private double Transform(double value, double max)
        {
            return (value * Max) / max;
        }

        private double TransformBack(double value, double max)
        {
            return (value * max) / Max;
        }

        private void VerifyInit()
        {
            if (splineCalc == null) throw new InvalidOperationException("SplineValues is not initialized.");
        }

        private void SortY()
        {
            int n = Points.Length;
            if (n == 2) return;
            do
            {
                int newN = 0;
                for (int i = 1; i <= n - 1; i++)
                {
                    if (Points[i - 1].Y > Points[i].Y)
                    {
                        int temp = Points[i - 1].Y;
                        Points[i - 1].Y = Points[i].Y;
                        Points[i].Y = temp;
                        newN = i;
                    }
                }
                n = newN;
            }
            while (n != 0);
        }

        public int ValuesCount
        {
            get { return (PointCount * 2) - 2; }
        }
    }
}
