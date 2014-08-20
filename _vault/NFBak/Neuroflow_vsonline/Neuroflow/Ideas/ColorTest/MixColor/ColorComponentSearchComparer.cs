using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using ColorTest;

namespace MixColor
{
    public sealed class ColorComponentSearchComparer : ColorComponentSearch, IComparer<Genom>, IParallelInitializableGenomComprarer
    {
        public ColorComponentSearchComparer(ColorFilteringPars filteringPars, ColorMixingPars mixingPars, HdrRGB targetColor, params BaseColor[] baseColors) :
            base(filteringPars, mixingPars, targetColor, baseColors)
        {
            Contract.Requires(filteringPars != null);
            Contract.Requires(mixingPars != null);
            Contract.Requires(baseColors != null);
            Contract.Requires(baseColors.Length > 0);
        }

        void IParallelInitializableGenomComprarer.InitializeGenomComparing(Genom g)
        {
            g.Data = CalculateMixingResult(g);
        }

        private ColorComponentSearchResult CalculateMixingResult(Genom g)
        {
            const double maxWeight = 5.0;

            var colors = new List<WeightedValue<HdrRGB>>(BaseColors.Length);
            for (int i = 0; i < BaseColors.Length; i++)
            {
                var gene = Math.Round(g.Genes[i] * maxWeight, 2, MidpointRounding.AwayFromZero);
                if (gene != 0.0) colors.Add(new WeightedValue<HdrRGB>(BaseColors[i].Color, gene));
            }

            return ComputeResult(colors.ToArray());
        }

        private ColorComponentSearchResult ComputeResult2(WeightedValue<HdrRGB>[] colors)
        {
            var result = new ColorComponentSearchResult();
            result.DirectResult = new MixedColor(TargetColor,
                new HdrRGB(
                    colors.Select(c => c.Weight * c.Value.R).Sum() / colors.Select(c => c.Weight).Sum(),
                    colors.Select(c => c.Weight * c.Value.G).Sum() / colors.Select(c => c.Weight).Sum(),
                    colors.Select(c => c.Weight * c.Value.B).Sum() / colors.Select(c => c.Weight).Sum()));
            result.FinalResult = result.DirectResult;
            return result;
        }

        public int Compare(Genom x, Genom y)
        {
            var xr = GetMixedResult(x);
            if (xr == null) xr = CalculateMixingResult(x);
            var yr = GetMixedResult(y);
            if (yr == null) yr = CalculateMixingResult(y);

            return xr.CompareTo(yr);
        }

        private ColorComponentSearchResult GetMixedResult(Genom g)
        {
            return g.Data as ColorComponentSearchResult;
        }
    }
}
