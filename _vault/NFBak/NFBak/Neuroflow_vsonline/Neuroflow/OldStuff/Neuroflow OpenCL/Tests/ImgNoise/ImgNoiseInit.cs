using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ImgNoise.Features;
using ImgNoise.Properties;
using Neuroflow.Networks.Neural;

namespace ImgNoise
{
    public static class ImgNoiseInit
    {
        public static int FFInputInterfaceLenght
        {
            get
            {
                var ss = Settings.Default.SampleSize;
                return ss * ss + 1;
            }
        }

        public static int RInputInterfaceLenght
        {
            get
            {
                var ss = Settings.Default.SampleSize;
                return ss + 1;
            }
        }

        public static int FFNeuronCount
        {
            get { return Settings.Default.FFNeuronCount; }
        }

        public static int RNeuronCount
        {
            get { return Settings.Default.RNeuronCount; }
        }

        public static IUnorderedNeuralVectorFlowProvider CreateFFProvider()
        {
            var searchPars = new SearchingParams(Settings.Default.ImagePaths.Split(';'), new[] { "*.jpg", "*.png" }, true);
            var samplPars = new SamplingParams(Settings.Default.SampleSize, Settings.Default.RecurrentSampleLength, 10);
            return new NoisedImagePartProvider(searchPars, samplPars, Settings.Default.SampleCount);
        }

        public static IUnorderedNeuralVectorFlowProvider CreateFFValidationProvider()
        {
            var searchPars = new SearchingParams(Settings.Default.ImagePaths.Split(';'), new[] { "*.jpg", "*.png" }, true);
            var samplPars = new SamplingParams(Settings.Default.SampleSize, Settings.Default.RecurrentSampleLength, 10);
            return new NoisedImagePartProvider(searchPars, samplPars, Settings.Default.ValidationSampleCount);
        }

        public static IUnorderedNeuralVectorFlowProvider CreateRProvider()
        {
            var searchPars = new SearchingParams(Settings.Default.ImagePaths.Split(';'), new[] { "*.jpg", "*.png" }, true);
            var samplPars = new SamplingParams(Settings.Default.SampleSize, Settings.Default.RecurrentSampleLength, 10);
            return new NoisedImageLineProvider(searchPars, samplPars, Settings.Default.SampleCount);
        }

        public static IUnorderedNeuralVectorFlowProvider CreateRValidationProvider()
        {
            var searchPars = new SearchingParams(Settings.Default.ImagePaths.Split(';'), new[] { "*.jpg", "*.png" }, true);
            var samplPars = new SamplingParams(Settings.Default.SampleSize, Settings.Default.RecurrentSampleLength, 10);
            return new NoisedImageLineProvider(searchPars, samplPars, Settings.Default.ValidationSampleCount);
        }
    }
}
