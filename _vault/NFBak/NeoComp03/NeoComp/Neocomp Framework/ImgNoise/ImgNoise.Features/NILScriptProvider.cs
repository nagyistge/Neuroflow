using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Learning;
using System.Diagnostics.Contracts;
using NeoComp.Core;

namespace ImgNoise.Features
{
    public sealed class NILScriptProvider : ScriptCollectionProvider
    {
        public NILScriptProvider(SearchingParams searchingPars, SamplingParams samplingPars, int maxSampleCount)
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
            readers = Search.FindNILReaders(searchingPars, samplingPars).OrderByRandom().Take(maxSampleCount).ToArray();
        }

        private Script CreateScript(int index)
        {
            var reader = readers[index];

            var entries = new LinkedList<ScriptEntry>();
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
                entries.AddLast(new ScriptEntry(inputVector, outputVector));
            }

            return new Script(entries);
        }

        private byte GetPixel(byte[] bytes)
        {
            int half = samplingPars.Size / 2;
            return bytes[half * samplingPars.Size + half];
        }
    }
}
