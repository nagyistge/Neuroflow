using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ColorTest
{
    public struct MixedColor : IEquatable<MixedColor>, IComparable<MixedColor>
    {
        public MixedColor(HdrRGB reference, HdrRGB mixed)
        {
            this.reference = reference;
            this.mixed = mixed;
        }

        HdrRGB reference, mixed;

        public HdrRGB Reference
        {
            get { return reference; }
        }

        public HdrRGB Mixed
        {
            get { return mixed; }
        }

        public double RDiff
        {
            get { return Math.Abs(reference.R - mixed.R); }
        }

        public double GDiff
        {
            get { return Math.Abs(reference.G - mixed.G); }
        }

        public double BDiff
        {
            get { return Math.Abs(reference.B - mixed.B); }
        }

        public double SumDiff
        {
            get { return RDiff + GDiff + BDiff; }
        }

        public double AvgDiff
        {
            get { return (RDiff + GDiff + BDiff) / 3.0; }
        }

        public double Error
        {
            get { return RDiff * RDiff + GDiff * GDiff + BDiff * BDiff; }
        }

        public double GetRoundedError(int decimals)
        {
            return Math.Round(Error, decimals, MidpointRounding.AwayFromZero);
        }

        public bool Equals(MixedColor other)
        {
            return reference == other.reference && mixed == other.mixed;
        }

        public int CompareTo(MixedColor other)
        {
            return Error.CompareTo(other.Error);
        }

        public override bool Equals(object obj)
        {
            return obj is MixedColor ? Equals((MixedColor)obj) : false;
        }

        public override string ToString()
        {
            return string.Format("[{0}] vs [{1}]", reference.ToString(0), mixed.ToString(0));
        }

        public override int GetHashCode()
        {
            return reference.GetHashCode() ^ mixed.GetHashCode();
        }

        public static bool operator ==(MixedColor c1, MixedColor c2)
        {
            return c1.Equals(c2);
        }

        public static bool operator !=(MixedColor c1, MixedColor c2)
        {
            return !c1.Equals(c2);
        }

        public static bool operator <(MixedColor c1, MixedColor c2)
        {
            return c1.CompareTo(c2) < 0;
        }

        public static bool operator <=(MixedColor c1, MixedColor c2)
        {
            return c1.CompareTo(c2) <= 0;
        }

        public static bool operator >(MixedColor c1, MixedColor c2)
        {
            return c1.CompareTo(c2) > 0;
        }

        public static bool operator >=(MixedColor c1, MixedColor c2)
        {
            return c1.CompareTo(c2) >= 0;
        }
    }
}
