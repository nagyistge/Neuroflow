using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Features;
using System.Diagnostics.Contracts;
using NeoComp.Core;

namespace ImgNoise.Features
{
    // TODO: NoiseLevel Description Override: 0..1!
    public sealed class NIPDataProvider : ObjectFeatureProvider
    {
        #region Strings

        public static readonly Strings InputFeatureIDs = new[] { "Reader.NoiseLevel", "NoisedImageData" };

        public static readonly Strings OutputFeatureIDs = new[] { "Pixel" };

        static readonly Strings Projections = new[] { "Reader.NoiseLevel", "Pixel", "NoisedImageData#" }; 

        #endregion

        #region Constructors

        public NIPDataProvider(SearchingParams searchingPars, SamplingParams samplingPars)
            : base(ToList(searchingPars, samplingPars, null), Projections, null)
        {
            Contract.Requires(!searchingPars.IsEmpty);
            Contract.Requires(!samplingPars.IsEmpty);
        }

        public NIPDataProvider(SearchingParams searchingPars, SamplingParams samplingPars, int maxSampleCount)
            : base(ToList(searchingPars, samplingPars, maxSampleCount), Projections, null)
        {
            Contract.Requires(!searchingPars.IsEmpty);
            Contract.Requires(!samplingPars.IsEmpty);
            Contract.Requires(maxSampleCount > 0);
        }

        // Helpers:
        private static List<NIPData> ToList(SearchingParams searchingPars, SamplingParams samplingPars, int? maxSampleCount)
        {
            Contract.Ensures(Contract.Result<List<NIPData>>() != null);
            Contract.Ensures(Contract.Result<List<NIPData>>().Count > 0);

            List<NIPData> result = null;

            var readers = Search.FindReaders(searchingPars, samplingPars);
            if (maxSampleCount == null)
            {
                result = readers.Select(r => new NIPData(r)).ToList();
            }
            else
            {
                result = readers.OrderByRandom().Take(maxSampleCount.Value).Select(r => new NIPData(r)).ToList();
            }

            if (result.Count == 0)
            {
                throw new InvalidOperationException("Samples not found.");
            }

            return result;
        } 

        #endregion
    }
}
