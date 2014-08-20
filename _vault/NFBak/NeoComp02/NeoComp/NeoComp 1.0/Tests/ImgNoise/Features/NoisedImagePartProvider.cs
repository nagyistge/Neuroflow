using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Optimizations.NeuralNetworks;
using NeoComp;
using NeoComp.Optimizations;

namespace ImgNoise.Features
{
    [Serializable]
    public sealed class NoisedImagePartProvider : UnorderedNeuralVectorsProvider
    {
        public NoisedImagePartProvider(SearchingParams searchingPars, SamplingParams samplingPars, int maxSampleCount)
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

        protected override int Initialize()
        {
            readers = Search.FindNIPReaders(searchingPars, samplingPars).OrderByRandom().Take(maxSampleCount).ToArray();
            return readers.Length;
        }

        protected override IEnumerable<NeuralVectors> DoGetNextVectors(IndexSet indexes)
        {
            foreach (var index in indexes) yield return GetVectors(index);
        }

        private NeuralVectors GetVectors(int index)
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

            return new NeuralVectors(new VectorFlowEntry<double>(inputVector, outputVector));
        }

        private byte GetPixel(byte[] bytes)
        {
            int half = samplingPars.Size / 2;
            return bytes[half * samplingPars.Size + half];
        }
    }
}
