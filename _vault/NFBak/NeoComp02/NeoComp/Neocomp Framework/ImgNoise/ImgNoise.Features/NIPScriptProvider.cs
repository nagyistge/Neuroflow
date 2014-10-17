using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Learning;
using System.Diagnostics.Contracts;
using NeoComp.Core;

namespace ImgNoise.Features
{
    public sealed class NIPScriptProvider : ScriptCollectionProvider
    {
        public NIPScriptProvider(SearchingParams searchingPars, SamplingParams samplingPars, int maxSampleCount)
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

        public override int Count
        {
            get { return readers == null ? 0 : readers.Length; }
        }

        public override Script this[int index]
        {
            get 
            {
                if (readers == null) throw new InvalidOperationException(this + " is not initalized.");
                return CreateScript(index); 
            }
        }

        public override void Reinitialize()
        {
            readers = Search.FindNIPReaders(searchingPars, samplingPars).OrderByRandom().Take(maxSampleCount).ToArray();
        }

        private Script CreateScript(int index)
        {
            var reader = readers[index];
            byte[] id, nid;
            reader.ReadNoised(out id, out nid);
            byte pixel = GetPixel(id);

            double?[] inputVector = new double?[nid.Length + 1];
            inputVector[0] = Helpers.GetNoiseLevel(reader.NoiseLevel);
            for (int idx = 0; idx < nid.Length; idx++) inputVector[idx + 1] = Helpers.PixelToDouble(nid[idx]);
            double?[] outputVector = new double?[1];
            outputVector[0] = Helpers.PixelToDouble(pixel);

            return new Script(new ScriptEntry(inputVector, outputVector));
        }

        private byte GetPixel(byte[] bytes)
        {
            int half = samplingPars.Size / 2;
            return bytes[half * samplingPars.Size + half];
        }
    }
}
