using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Drawing;

namespace ImgNoise.Features
{
    [Serializable]
    public sealed class NoisedImagePartReader : ImagePartReader
    {
        #region Constructor

        public NoisedImagePartReader(string sourcePath, Point position, int size, Channel channel, float noiseLevel)
            : base(sourcePath, position, size, channel)
        {
            Contract.Requires(!String.IsNullOrEmpty(sourcePath));
            Contract.Requires(!position.IsEmpty);
            Contract.Requires(position.X >= 0);
            Contract.Requires(position.Y >= 0);
            Contract.Requires(size > 0);
            Contract.Requires(noiseLevel >= 0.0 && noiseLevel <= 1.0);

            NoiseLevel = noiseLevel;
        }

        #endregion
        
        #region Properties and Fields

        public float NoiseLevel { get; private set; }

        #endregion

        #region Read

        public byte[] ReadNoised()
        {
            Contract.Ensures(Contract.Result<byte[]>().Length == Size * Size);

            var bytes = base.Read();
            for (int idx = 0; idx < bytes.Length; idx++)
            {
                bytes[idx] = Noise.Add(bytes[idx], NoiseLevel);
            }
            return bytes;
        }

        public void ReadNoised(out byte[] originalBytes, out byte[] noisedBytes)
        {
            Contract.Ensures(Contract.ValueAtReturn(out originalBytes) != null);
            Contract.Ensures(Contract.ValueAtReturn(out noisedBytes) != null);
            Contract.Ensures(Contract.ValueAtReturn(out originalBytes).Length == Size * Size);
            Contract.Ensures(Contract.ValueAtReturn(out noisedBytes).Length == Size * Size);
            
            originalBytes = base.Read();
            noisedBytes = new byte[originalBytes.Length];
            for (int idx = 0; idx < originalBytes.Length; idx++)
            {
                noisedBytes[idx] = Noise.Add(originalBytes[idx], NoiseLevel);
            }
        }

        #endregion
    }
}
