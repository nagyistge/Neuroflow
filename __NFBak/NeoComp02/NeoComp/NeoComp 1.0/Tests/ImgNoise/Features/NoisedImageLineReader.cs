using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Diagnostics.Contracts;

namespace ImgNoise.Features
{
    public struct Column
    {
        public Column(byte[] inputPixels, byte? outputPixel = null)
        {
            Contract.Requires(inputPixels != null && inputPixels.Length > 0);

            this.inputPixels = inputPixels;
            this.outputPixel = outputPixel;
        }
        
        byte[] inputPixels;

        public byte[] InputPixels
        {
            get { return inputPixels; }
        }

        byte? outputPixel;

        public byte? OutputPixel
        {
            get { return outputPixel; }
        }
    }
    
    [Serializable]
    public class NoisedImageLineReader : ImageReader
    {
        #region Constructor

        public NoisedImageLineReader(string sourcePath, Point position, int size, int width, Channel channel, double noiseLevel)
            : base(sourcePath, position, size, channel)
        {
            Contract.Requires(!String.IsNullOrEmpty(sourcePath));
            Contract.Requires(!position.IsEmpty);
            Contract.Requires(position.X >= 0);
            Contract.Requires(position.Y >= 0);
            Contract.Requires(size > 0 && size % 2 == 1);
            Contract.Requires(width > (size / 2) * 2);
            Contract.Requires(noiseLevel >= 0.0 && noiseLevel <= 1.0);

            Width = width;
            NoiseLevel = noiseLevel;
        }

        #endregion

        #region Properties

        public int Width { get; private set; }

        public double NoiseLevel { get; private set; }

        #endregion

        #region Read

        public IEnumerable<Column> Read()
        {
            var bmp = GetBitmap();
            VerifySize(bmp.Width, bmp.Height);

            int size2 = Size / 2;
            byte[] buff = new byte[Size];
            int outIdx = 0;
            for (int x = Position.X; x < Position.X + Width; x++, outIdx++)
            {
                int buffIdx = 0;
                for (int y = Position.Y; y < Position.Y + Size; y++, buffIdx++)
                {
                    buff[buffIdx] = Noise.Add(GetCahnnelValue(bmp.SafeGetPixel(x, y)), NoiseLevel);
                }
                
                byte? output = null;
                if (outIdx >= Size - 1)
                {
                    output = GetCahnnelValue(bmp.SafeGetPixel(x - size2, Position.Y + size2));
                }
                yield return new Column(buff, output);
            }
        }

        private void VerifySize(int w, int h)
        {
            if (w < Position.X + Width || h < Position.Y + Size) throw new InvalidOperationException("Bitmap size is invalid.");
        }

        #endregion
    }
}
