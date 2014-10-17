using ColorTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace ComputeFreeParameters
{
    public class ColorMixerGenomComparer : GeneBasedColorMixer, IComparer<Genom>, ICloneable, IParallelInitializableGenomComprarer
    {
        public ColorMixerGenomComparer(IEnumerable<ColorRecipe> recipes) :
            base(recipes)
        {
            if (Recipes.Count == 0) throw new ArgumentException("Recipes collection is empty.", "recipes");
        }

        public int Compare(Genom x, Genom y)
        {
            MixedColorsResult xr;
            MixedColorsResult yr;
            GetResults(x, y, out xr, out yr);
            int c = xr.MaxError.CompareTo(yr.MaxError);
            if (c == 0) c = xr.AvgError.CompareTo(yr.AvgError);
            if (c == 0) c = xr.MinError.CompareTo(yr.MinError);
            return c;
        }

        private void GetResults(Genom x, Genom y, out MixedColorsResult xr, out MixedColorsResult yr)
        {
            xr = GetResult(x);
            yr = GetResult(y);

            if (xr == null)
            {
                if (yr == null)
                {
                    MixedColorsResult lxr = null, lyr = null;
                    Parallel.Invoke(() => lxr = CreateMixedColors(x), () => lyr = CreateMixedColors(y));
                    xr = lxr;
                    yr = lyr;
                }
                else
                {
                    xr = CreateMixedColors(x);
                }
            }
            else if (yr == null)
            {
                yr = CreateMixedColors(y);
            }

            Debug.Assert(xr != null);
            Debug.Assert(yr != null);
        }

        private MixedColorsResult GetResult(Genom g)
        {
            return g.Data as MixedColorsResult;
        }

        public void InitializeGenomComparing(Genom g)
        {
            g.Data = CreateMixedColors(g);
        }

        public object Clone()
        {
            return new ColorMixerGenomComparer(Recipes);
        }
    }
}
