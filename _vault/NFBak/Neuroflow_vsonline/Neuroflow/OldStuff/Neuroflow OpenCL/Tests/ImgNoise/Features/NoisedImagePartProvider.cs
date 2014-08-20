using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Neuroflow.Core;
using Neuroflow.Networks.Neural;
using Neuroflow.Core.Vectors;

namespace ImgNoise.Features
{
    [Serializable]
    public sealed class NoisedImagePartProvider : UnorderedNeuralVectorFlowProvider
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

        protected override UnorderedNeuralVectorFlowProvider.InitializationData Initialize()
        {
            readers = Search.FindNIPReaders(searchingPars, samplingPars).OrderByRandom().Take(maxSampleCount).ToArray();
            return new InitializationData(readers.Length, samplingPars.Size * samplingPars.Size + 1, 1);
        }

        protected override IEnumerable<NeuralVectorFlow> DoGetNext(IndexSet indexes)
        {
            return indexes.AsParallel().Select(idx => GetVectors(idx));
        }

        private NeuralVectorFlow GetVectors(int index)
        {
            var reader = readers[index];
            byte[] id, nid;
            reader.ReadNoised(out id, out nid);
            byte pixel = GetPixel(id);

            float[] inputVector = new float[nid.Length + 1];
            inputVector[0] = Helpers.GetNoiseLevel(reader.NoiseLevel);
            for (int idx = 0; idx < nid.Length; idx++) inputVector[idx + 1] = Helpers.PixelToDouble(nid[idx]);
            float?[] outputVector = new float?[1];
            outputVector[0] = Helpers.PixelToDouble(pixel);

            return new NeuralVectorFlow(index, new VectorFlowEntry<float>(inputVector, outputVector));
        }

        private byte GetPixel(byte[] bytes)
        {
            int half = samplingPars.Size / 2;
            return bytes[half * samplingPars.Size + half];
        }
    }
}
