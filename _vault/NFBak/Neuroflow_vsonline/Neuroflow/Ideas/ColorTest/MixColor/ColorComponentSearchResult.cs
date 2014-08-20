using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorTest;

namespace MixColor
{
    public class ColorComponentSearchResult : IComparable<ColorComponentSearchResult>
    {
        public MixedColor FinalResult { get; internal set; }

        public MixedColor DirectResult { get; internal set; }

        public double AdjusmentMod { get; internal set; }

        public int MaxWeight { get; internal set; }

        public int CompareTo(ColorComponentSearchResult other)
        {
            int c;
            c = DirectResult.Error.CompareTo(other.DirectResult.Error);
            //if (c == 0) FinalResult.GetRoundedError(errorDecimals).CompareTo(other.FinalResult.GetRoundedError(errorDecimals));
            //if (c == 0) c = MaxWeight.CompareTo(other.MaxWeight);
            //if (c == 0) c = Round(AdjusmentMod, 2).CompareTo(Round(other.AdjusmentMod, 2));
            //if (c == 0) FinalResult.Error.CompareTo(other.FinalResult.Error);
            return c;
        }

        private double Round(double adjustmentMod, int errorDecimals)
        {
            return Math.Round(adjustmentMod, errorDecimals, MidpointRounding.AwayFromZero);
        }

        public override string ToString()
        {
            return string.Format(
                "FRE:{0}, DRE: {1}, AM: {2}, MW: {3}", 
                FinalResult.Error.ToString("0.00"), 
                DirectResult.Error.ToString("0.00"), 
                AdjusmentMod.ToString("0.00"),
                MaxWeight);
        }
    }
}
