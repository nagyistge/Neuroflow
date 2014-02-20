using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorTest;

namespace MixColor
{
    public class ColorComponentSearch
    {
        readonly static double[] TestPrimes = new[] { 1.0, 2.0, 3.0, 5.0, 7.0 };

        public ColorComponentSearch(ColorFilteringPars filteringPars, ColorMixingPars mixingPars, HdrRGB targetColor, params BaseColor[] baseColors)
        {
            Contract.Requires(filteringPars != null);
            Contract.Requires(mixingPars != null);
            Contract.Requires(baseColors != null);
            Contract.Requires(baseColors.Length > 0);

            FilteringPars = filteringPars;
            MixingPars = mixingPars;
            BaseColors = baseColors;
            TargetColor = targetColor;
        }

        public ColorFilteringPars FilteringPars { get; private set; }

        public ColorMixingPars MixingPars { get; private set; }

        public HdrRGB TargetColor { get; private set; }

        public BaseColor[] BaseColors { get; private set; }

        public ColorComponentSearchResult ComputeResult(WeightedValue<HdrRGB>[] colors)
        {
            Contract.Requires(colors != null);
            Contract.Requires(colors.Length != 0);

            var result = new ColorComponentSearchResult();

            //colors = new[] { new WeightedValue<HdrRGB>(new HdrRGB(213, 198, 185), 0) };

            var directMixed = ColorMixer.Mix(FilteringPars, MixingPars, colors);
            result.DirectResult = new MixedColor(TargetColor, directMixed);

            //double minWeight = mixDict.Values.Select(c => c.Weight).Min();
            //double bestSumMod = double.MaxValue;
            //WeightedBaseColor[] bestMixes = null;
            //foreach (double prime in TestPrimes)
            //{
            //    var mixes = new WeightedBaseColor[mixDict.Count];
            //    double sumMod = 0.0;
            //    int mixIndex = 0;
            //    foreach (var mix in mixDict.Values)
            //    {
            //        double newWeight = (mix.Weight * prime) / minWeight;
            //        double roundedNewWeight = Math.Round(newWeight, 0, MidpointRounding.AwayFromZero);
            //        mixes[mixIndex++] = new WeightedBaseColor(mix.Value, roundedNewWeight);
            //        sumMod += Math.Abs(newWeight - roundedNewWeight);
            //    }
            //    if (sumMod < bestSumMod)
            //    {
            //        bestSumMod = sumMod;
            //        bestMixes = mixes;
            //    }
            //}

            //result.AdjusmentMod = bestSumMod;
            //result.MaxWeight = (int)bestMixes.Max(c => c.Weight);

            //colors = bestMixes.Select(c => new WeightedValue<HdrRGB>(c.Value.Color, c.Weight)).ToArray();
            
            //var finalMixed = ColorMixer.Mix(FilteringPars, MixingPars, colors);
            //result.FinalResult = new MixedColor(TargetColor, finalMixed);

            result.FinalResult = result.DirectResult;

            return result;
        }

        private double GetMinDiff(HdrRGB color, out BaseColor bestBaseColor)
        {
            double minDiff = double.MaxValue;
            bestBaseColor = null;
            foreach (var baseColor in BaseColors)
            {
                double rDiff = Math.Abs(color.R - baseColor.Color.R);
                double gDiff = Math.Abs(color.G - baseColor.Color.G);
                double bDiff = Math.Abs(color.B - baseColor.Color.B);
                double d = (rDiff * rDiff + gDiff * gDiff + bDiff * bDiff) / 3.0;
                if (d < minDiff)
                {
                    minDiff = d;
                    bestBaseColor = baseColor;
                }
            }
            return minDiff / BaseColors.Length;
        }
    }
}
