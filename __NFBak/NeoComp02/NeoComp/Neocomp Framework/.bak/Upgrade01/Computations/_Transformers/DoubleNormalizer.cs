using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Computations
{
    public sealed class DoubleNormalizer : DoubleTransformer<double>
    {
        public DoubleNormalizer(double sourceMin, double sourceMax, double min = 0.0, double max = 1.0)
        {
            Contract.Requires(min < max);
            Contract.Requires(sourceMin < sourceMax);

            SourceMin = sourceMin;
            SourceMax = sourceMax;
            Min = min;
            Max = max;

            sd = SourceMax - SourceMin;
            d = Max - Min;
        }

        double sd, d;

        public double SourceMin { get; private set; }

        public double SourceMax { get; private set; }

        public double Min { get; private set; }

        public double Max { get; private set; }
        
        public override double Transform(double value)
        {
            if (value < SourceMin) return Min;
            if (value > SourceMax) return Max;

            double v = value - SourceMin;
            double x = (d * v) / sd;

            return x + Min;
        }
    }
}
