using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Diagnostics.Contracts;
using System.Diagnostics;
using System.Threading;
using NeoComp.Imaging;

namespace ImgNoise.Features
{
    [Serializable]
    public class ImagePartReader : ImageReader
    {
        #region Constructor

        public ImagePartReader(string sourcePath, Point position, int size, Channel channel)
            : base(sourcePath, position, size, channel)
        {
            Contract.Requires(!String.IsNullOrEmpty(sourcePath));
            Contract.Requires(!position.IsEmpty);
            Contract.Requires(position.X >= 0);
            Contract.Requires(position.Y >= 0);
            Contract.Requires(size > 0);
        }

        #endregion

        #region Read

        public byte[] Read()
        {
            Contract.Ensures(Contract.Result<byte[]>() != null);
            Contract.Ensures(Contract.Result<byte[]>().Length == Size * Size); 
            
            try
            {
                var bytes = GetBytes();
                return bytes;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Cannot read image '" + SourcePath + "' part data. See inner exception for details.", ex);
            }
        }

        private byte[] GetBytes()
        {
            byte[] buff = new byte[Size * Size];
            var bmp = GetBitmap();
            int w = bmp.Width;
            int h = bmp.Height;
            VerifySize(w, h);

            int buffIdx = 0;
            int xs = Position.X + Size;
            int ys = Position.Y + Size;
            for (int y = Position.Y; y < ys; y++)
            {
                for (int x = Position.X; x < xs; x++)
                {
                    var px = bmp.SafeGetPixel(x, y);
                    switch (Channel)
                    {
                        case Channel.Red:
                            buff[buffIdx++] = px.R;
                            break;
                        case Channel.Green:
                            buff[buffIdx++] = px.G;
                            break;
                        default:
                            buff[buffIdx++] = px.B;
                            break;
                    }
                }
            }
            return buff;
        }

        private void VerifySize(int width, int height)
        {
            if (width < Position.X + Size) throw new InvalidOperationException("Horizontal size not fits bitmap size.");
            if (height < Position.Y + Size) throw new InvalidOperationException("Vertical size not fits bitmap size.");
        }

        #endregion
    }
}
