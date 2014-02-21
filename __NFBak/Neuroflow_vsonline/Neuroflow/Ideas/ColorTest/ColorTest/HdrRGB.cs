using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ColorTest
{
    public struct HdrRGB : IEquatable<HdrRGB>
    {
        public HdrRGB(byte r, byte g, byte b)
        {
            this.r = (double)r;
            this.g = (double)g;
            this.b = (double)b;
        }

        public HdrRGB(int r, int g, int b)
        {
            this.r = Math.Min(Math.Max((double)r, 0.0), 255.0);
            this.g = Math.Min(Math.Max((double)g, 0.0), 255.0);
            this.b = Math.Min(Math.Max((double)b, 0.0), 255.0);
        }

        public HdrRGB(double r, double g, double b)
        {
            this.r = Math.Min(Math.Max(r, 0.0), 255.0);
            this.g = Math.Min(Math.Max(g, 0.0), 255.0);
            this.b = Math.Min(Math.Max(b, 0.0), 255.0);
        }

        public HdrRGB(Color color)
        {
            r = (double)color.R;
            g = (double)color.G;
            b = (double)color.B;
        }

        public HdrRGB(HdrCMYK cmyk)
        {
            r = (1.0 - cmyk.C) * (1.0 - cmyk.K) * 255.0;
            g = (1.0 - cmyk.M) * (1.0 - cmyk.K) * 255.0;
            b = (1.0 - cmyk.Y) * (1.0 - cmyk.K) * 255.0;
        }

        private double r;

        public double R
        {
            get { return r; }
            set { r = Math.Min(Math.Max(value, 0.0), 255.0); }
        }

        private double g;

        public double G
        {
            get { return g; }
            set { g = Math.Min(Math.Max(value, 0.0), 255.0); }
        }

        private double b;

        public double B
        {
            get { return b; }
            set { b = Math.Min(Math.Max(value, 0.0), 255.0); }
        }

        public bool Equals(HdrRGB other)
        {
            return R == other.R && G == other.G && B == other.B;
        }

        public override bool Equals(object obj)
        {
            return obj is HdrRGB ? Equals((HdrRGB)obj) : false;
        }

        public override int GetHashCode()
        {
            return R.GetHashCode() ^ G.GetHashCode() ^ B.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0},{1},{2}", R.ToString("0.00"), G.ToString("0.00"), B.ToString("0.00"));
        }

        public string ToString(int round)
        {
            string format;
            if (round <= 0)
            {
                format = "0";
            }
            else
            {
                format = "0." + new string(Enumerable.Repeat('0', round).ToArray());
            }
            return string.Format("{0},{1},{2}", R.ToString(format), G.ToString(format), B.ToString(format));
        }

        public static bool operator ==(HdrRGB rgb1, HdrRGB rgb2)
        {
            return rgb1.Equals(rgb2);
        }

        public static bool operator !=(HdrRGB rgb1, HdrRGB rgb2)
        {
            return !rgb1.Equals(rgb2);
        }

        public static explicit operator Color(HdrRGB rgb)
        {
            return Color.FromRgb((byte)Math.Round(rgb.R, MidpointRounding.AwayFromZero), (byte)Math.Round(rgb.G, MidpointRounding.AwayFromZero), (byte)Math.Round(rgb.B, MidpointRounding.AwayFromZero));
        }

        public static explicit operator HdrRGB(Color color)
        {
            return new HdrRGB(color);
        }

        public static implicit operator HdrCMYK(HdrRGB rgb)
        {
            return new HdrCMYK(rgb);
        }

        public static implicit operator HdrRGB(HdrCMYK cmyk)
        {
            return new HdrRGB(cmyk);
        }
    }
}
