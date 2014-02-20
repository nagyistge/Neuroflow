using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorTest;

namespace ComputeFreeParameters
{
    public sealed class MixedColorsResult
    {
        internal MixedColorsResult(IEnumerable<MixedColor> colors)
        {
            Contract.Requires(colors != null);

            var a = colors.ToArray();

            if (a.Length == 0) throw new ArgumentException("Colors collection is empty.", "colors");

            Array.Sort(a);
            MixedColors = Array.AsReadOnly(a);
            MinError = a[0].Error;
            MaxError = a[a.Length - 1].Error;
            AvgError = a.Average(i => i.Error);
        }

        public ReadOnlyCollection<MixedColor> MixedColors { get; private set; }

        public double MinError { get; private set; }

        public double MaxError { get; private set; }

        public double AvgError { get; private set; }

        public MixedColor WorstMix
        {
            get { return MixedColors[MixedColors.Count - 1]; }
        }

        public MixedColor BestMix
        {
            get { return MixedColors[0]; }
        }
    }
}
