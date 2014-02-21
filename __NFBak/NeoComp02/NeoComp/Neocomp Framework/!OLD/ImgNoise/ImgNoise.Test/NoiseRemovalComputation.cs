using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks.Computational.Neural;
using System.Diagnostics.Contracts;
using ImgNoise.Features;
using NeoComp.Features;

namespace ImgNoise.Test
{
    public sealed class NoiseRemovalComputation
    {
        public NoiseRemovalComputation(NeuralComputation comp)
        {
            Contract.Requires(comp != null);

            BaseComputation = comp;
        }

        public NeuralComputation BaseComputation { get; private set; }

        //public byte Compute(byte[] noisedImageBytes)
        //{
        //    Contract.Requires(noisedImageBytes != null);
        //    Contract.Requires(noisedImageBytes.Length != 0);
        //    Contract.Ensures(Contract.Result<byte[]>() != null);
        //    Contract.Ensures(Contract.Result<byte[]>().Length == noisedImageBytes.Length);

        //    var compResult = BaseComputation.Compute(noisedImageBytes);
        //    return (byte)Math.Round((double)compResult[0]);
        //}

        public byte Compute(byte[] noisedImageBytes, double noiseLevel)
        {
            Contract.Requires(noisedImageBytes != null);
            Contract.Requires(noisedImageBytes.Length != 0);
            Contract.Ensures(Contract.Result<byte[]>() != null);
            Contract.Ensures(Contract.Result<byte[]>().Length == noisedImageBytes.Length);
            Contract.Requires(noiseLevel >= 0.0 && noiseLevel <= global::ImgNoise.Features.Properties.Settings.Default.MaxNoiseLevel);

            var compResult = BaseComputation.Compute(noiseLevel, noisedImageBytes);
            return (byte)Math.Round((double)compResult[0]);
        }
    }
}
