using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Neuroflow.Core.Optimizations.NeuralNetworks;
using Neuroflow.Core.Optimizations;
using Neuroflow.Core;

namespace ImgNoise.Features
{
    [Serializable]
    public sealed class NoisedImageLineProvider : UnorderedNeuralVectorsProvider
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

        protected override UnorderedNeuralVectorsProvider.InitializationData Initialize()
        {
            readers = Search.FindNILReaders(searchingPars, samplingPars).OrderByRandom().Take(maxSampleCount).ToArray();

            return new InitializationData(readers.Length, samplingPars.Size + 1, 1);
        }

        protected override IEnumerable<NeuralVectors> DoGetNextVectors(IndexSet indexes)
        {
            foreach (var index in indexes) yield return GetVectors(index);
        }

        private NeuralVectors GetVectors(int index)
        {
            var reader = readers[index];

            var entries = new LinkedList<VectorFlowEntry<double>>();
            int size = reader.Size;
            foreach (var col in reader.Read())
            {
                double?[] inputVector = new double?[size + 1];
                inputVector[0] = Helpers.GetNoiseLevel(reader.NoiseLevel);
                for (int y = 0; y < size; y++) inputVector[y + 1] = Helpers.PixelToDouble(col.InputPixels[y]);

                double?[] outputVector = null;
                if (col.OutputPixel != null)
                {
                    outputVector = new double?[1];
                    outputVector[0] = Helpers.PixelToDouble(col.OutputPixel.Value);
                }
                entries.AddLast(new VectorFlowEntry<double>(inputVector, outputVector));
            }

            return new NeuralVectors(index, entries);
        }
    }
}
