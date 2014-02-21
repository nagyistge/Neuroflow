using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorTest
{
    public struct HdrCMYK : IEquatable<HdrCMYK>
    {
        public HdrCMYK(double c, double m, double y, double k)
        {
            this.c = Math.Min(Math.Max(c, 0.0), 1.0);
            this.m = Math.Min(Math.Max(m, 0.0), 1.0);
            this.y = Math.Min(Math.Max(y, 0.0), 1.0);
            this.k = Math.Min(Math.Max(k, 0.0), 1.0);
        }

        public HdrCMYK(HdrRGB rgb)
        {
            c = (255.0 - rgb.R) / 255.0;
            m = (255.0 - rgb.G) / 255.0;
            y = (255.0 - rgb.B) / 255.0;

            k = Math.Min(c, Math.Min(m, y));
            if (k == 1.0)
            {
                c = m = y = 0.0;
            }
            else
            {
                double k2 = 1.0 - k;
                c = (c - k) / k2;
                m = (m - k) / k2;
                y = (y - k) / k2;
            }
        }

        private double c;

        public double C
        {
            get { return c; }
            set { c = Math.Min(Math.Max(value, 0.0), 1.0); }
        }

        private double m;

        public double M
        {
            get { return m; }
            set { m = Math.Min(Math.Max(value, 0.0), 1.0); }
        }

        private double y;

        public double Y
        {
            get { return y; }
            set { y = Math.Min(Math.Max(value, 0.0), 1.0); }
        }

        private double k;

        public double K
        {
            get { return k; }
            set { k = Math.Min(Math.Max(value, 0.0), 1.0); }
        }

        public bool Equals(HdrCMYK other)
        {
            return C == other.C && M == other.M && Y == other.Y && K == other.K;
        }

        public override bool Equals(object obj)
        {
            return obj is HdrCMYK ? Equals((HdrCMYK)obj) : false;
        }

        public override int GetHashCode()
        {
            return C.GetHashCode() ^ M.GetHashCode() ^ Y.GetHashCode() ^ K.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0},{1},{2},{3}", C.ToString("0.00"), M.ToString("0.00"), Y.ToString("0.00"), K.ToString("0.00"));
        }

        public static bool operator ==(HdrCMYK rgb1, HdrCMYK rgb2)
        {
            return rgb1.Equals(rgb2);
        }

        public static bool operator !=(HdrCMYK rgb1, HdrCMYK rgb2)
        {
            return !rgb1.Equals(rgb2);
        }
    }
}
