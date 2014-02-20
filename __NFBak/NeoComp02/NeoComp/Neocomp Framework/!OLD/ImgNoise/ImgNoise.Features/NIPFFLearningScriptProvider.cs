using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Learning;
using System.Diagnostics.Contracts;
using NeoComp.Core;

namespace ImgNoise.Features
{
    public sealed class NIPFFLearningScriptProvider : LearningScriptCollectionProvider
    {
        public NIPFFLearningScriptProvider(SearchingParams searchingPars, SamplingParams samplingPars, int maxSampleCount)
        {
            Contract.Requires(!searchingPars.IsEmpty);
            Contract.Requires(!samplingPars.IsEmpty);
            Contract.Requires(maxSampleCount > 100);
            
            this.searchingPars = searchingPars;
            this.samplingPars = samplingPars;
            this.maxSampleCount = maxSampleCount;
        }

        SearchingParams searchingPars;

        SamplingParams samplingPars;

        int maxSampleCount;

        NoisedImagePartReader[] readers;

        static readonly DoubleRange targetRange = new DoubleRange(-1.0, 1.0);

        static readonly DoubleRange pixelRange = new DoubleRange(0.0, 255.0);

        public override int Count
        {
            get { return readers == null ? 0 : readers.Length; }
        }

        public override LearningScript this[int index]
        {
            get 
            {
                if (readers == null) throw new InvalidOperationException(this + " is not initalized.");
                return CreateScript(index); 
            }
        }

        public override bool SupportsReinitialize
        {
            get { return true; }
        }

        public override void Reinitialize()
        {
            readers = Search.FindReaders(searchingPars, samplingPars).OrderByRandom().Take(maxSampleCount).ToArray();
        }

        private LearningScript CreateScript(int index)
        {
            var reader = readers[index];
            byte[] id, nid;
            reader.ReadNoised(out id, out nid);
            byte pixel = GetPixel(id);

            double?[] inputVector = new double?[nid.Length];
            for (int idx = 0; idx < nid.Length; idx++) inputVector[idx] = ToDouble(nid[idx]);
            double?[] outputVector = new double?[1];
            outputVector[0] = ToDouble(pixel);

            return new LearningScript(new LearningScriptEntry(inputVector, outputVector));
        }

        private double ToDouble(byte pixel)
        {
            return pixelRange.Normalize((double)pixel, targetRange);
        }

        private byte GetPixel(byte[] bytes)
        {
            int half = samplingPars.Size / 2;
            return bytes[half * samplingPars.Size + half];
        }
    }
}
