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
    public sealed class NoisedImageLineProvider : UnorderedNeuralVectorFlowProvider
    {
        public NoisedImageLineProvider(SearchingParams searchingPars, SamplingParams samplingPars, int maxSampleCount)
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

        NoisedImageLineReader[] readers;

        protected override UnorderedNeuralVectorFlowProvider.InitializationData Initialize()
        {
            readers = Search.FindNILReaders(searchingPars, samplingPars).OrderByRandom().Take(maxSampleCount).ToArray();

            return new InitializationData(readers.Length, samplingPars.Size + 1, 1);
        }

        protected override IEnumerable<NeuralVectorFlow> DoGetNext(IndexSet indexes)
        {
            foreach (var index in indexes) yield return GetVectors(index);
        }

        private NeuralVectorFlow GetVectors(int index)
        {
            var reader = readers[index];

            var entries = new LinkedList<VectorFlowEntry<float>>();
            int size = reader.Size;
            foreach (var col in reader.Read())
            {
                float[] inputVector = new float[size + 1];
                inputVector[0] = Helpers.GetNoiseLevel(reader.NoiseLevel);
                for (int y = 0; y < size; y++) inputVector[y + 1] = Helpers.PixelToDouble(col.InputPixels[y]);

                float?[] outputVector = null;
                if (col.OutputPixel != null)
                {
                    outputVector = new float?[1];
                    outputVector[0] = Helpers.PixelToDouble(col.OutputPixel.Value);
                }
                entries.AddLast(new VectorFlowEntry<float>(inputVector, outputVector));
            }

            return new NeuralVectorFlow(index, entries.ToArray());
        }
    }
}
