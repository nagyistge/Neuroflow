using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Optimizations.NeuralNetworks;
using ImgNoise.Features;

namespace WFTestConsole
{
    public static class ImgNoiseInit
    {
        public static int FFInputInterfaceLenght
        {
            get 
            {
                var ss = global::ImgNoise.Properties.Settings.Default.SampleSize;
                return ss * ss + 1; 
            }
        }

        public static int RInputInterfaceLenght
        {
            get
            {
                var ss = global::ImgNoise.Properties.Settings.Default.SampleSize;
                return ss + 1;
            }
        }

        public static int NeuronCount
        {
            get { return global::ImgNoise.Properties.Settings.Default.NeuronCount; }
        }

        public static IUnorderedNeuralVectorsProvider FFProvider
        {
            get
            {
                var searchPars = new SearchingParams(global::ImgNoise.Properties.Settings.Default.ImagePaths.Split(';'), new[] { "*.jpg", "*.png" }, true);
                var samplPars = new SamplingParams(global::ImgNoise.Properties.Settings.Default.SampleSize, global::ImgNoise.Properties.Settings.Default.RecurrentSampleLength, 10);
                return new NoisedImagePartProvider(searchPars, samplPars, global::ImgNoise.Properties.Settings.Default.SampleCount);
            }
        }
    }
}
