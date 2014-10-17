using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorTest;

namespace ComputeFreeParameters
{
    public sealed class CombinedColorMixerGenomComprarer : IComparer<Genom>
    {
        public CombinedColorMixerGenomComprarer(params GeneBasedColorMixer[] mixers)
        {
            Contract.Requires(mixers.Length > 0);

            this.mixers = mixers.ToArray();
        }

        GeneBasedColorMixer[] mixers;

        public int Compare(Genom x, Genom y)
        {
            double xrMaxError = 0, yrMaxError = 0, xrMinError = 0, yrMinError = 0;
            double xrAvgError = 0.0, yrAvgError = 0.0;
            for (int i = 0; i < mixers.Length; i++)
            {
                var mixer = mixers[i];
                var xr = mixer.CreateMixedColors(x);
                var yr = mixer.CreateMixedColors(y);

                xrMaxError += xr.MaxError;
                xrMinError += xr.MinError;
                xrAvgError += xr.AvgError;

                yrMaxError += yr.MaxError;
                yrMinError += yr.MinError;
                yrAvgError += yr.AvgError;
            }

            int c = xrMaxError.CompareTo(yrMaxError);
            if (c == 0) c = xrAvgError.CompareTo(yrAvgError);
            if (c == 0) c = xrMinError.CompareTo(yrMinError);
            return c;
        }
    }
}
