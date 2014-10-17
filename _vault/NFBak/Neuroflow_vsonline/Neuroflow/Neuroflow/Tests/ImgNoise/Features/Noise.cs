using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core;

namespace ImgNoise.Features
{
    public sealed class Noise
    {
        public static byte Add(byte b, float noiseLevel)
        {
            if (noiseLevel == 0.0) return b;
            float r = (float)Statistics.GenerateGauss(0.0, noiseLevel);
            int value = (int)Math.Round((float)b + (r * 255.0));
            if (value < 0) value = 0; else if (value > 255) value = 255;
            return (byte)value;
        }
    }
}
