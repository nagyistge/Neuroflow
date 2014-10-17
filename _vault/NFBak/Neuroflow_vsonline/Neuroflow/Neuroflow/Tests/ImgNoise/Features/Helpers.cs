using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImgNoise.Features
{
    internal static class Helpers
    {
        static readonly float maxNoiseLevel = Properties.Settings.Default.MaxNoiseLevel;
        
        internal static float GetNoiseLevel(float level)
        {
            return (level / maxNoiseLevel * 2.0f) - 1.0f;
        }
        
        internal static float PixelToDouble(byte pixel)
        {
            return ((float)pixel / 255.0f) * 2.0f - 1.0f;
        }

        internal static byte DoubleToPixel(float value)
        {
            int v = (int)Math.Round(((value + 1.0) / 2.0) * 255.0);
            if (v < 0) v = 0; else if (v > 255) v = 255;
            return (byte)v;
        }
    }
}
