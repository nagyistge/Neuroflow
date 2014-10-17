using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImgNoise.Features
{
    internal static class Helpers
    {
        internal static double GetNoiseLevel(double level)
        {
            return (level / Properties.Settings.Default.MaxNoiseLevel) * 2.0 - 1.0;
        }
        
        internal static double PixelToDouble(byte pixel)
        {
            return ((double)pixel / 255.0) * 2.0 - 1.0;
        }

        internal static byte DoubleToPixel(double value)
        {
            int v = (int)Math.Round(((value + 1.0) / 2.0) * 255.0);
            if (v < 0) v = 0; else if (v > 255) v = 255;
            return (byte)v;
        }
    }
}
