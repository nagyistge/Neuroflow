using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Diagnostics.Contracts;
using System.Diagnostics;
using System.Threading;
using NeoComp.Caching;
using System.Web.Caching;
using NeoComp.Imaging;

namespace ImgNoise.Features
{
    public enum Channel { Red, Green, Blue }
    
    public class ImagePartReader
    {
        #region Constructor

        public ImagePartReader(string sourcePath, Point position, int size, Channel channel)
        {
            Contract.Requires(!String.IsNullOrEmpty(sourcePath));
            Contract.Requires(!position.IsEmpty);
            Contract.Requires(position.X >= 0);
            Contract.Requires(position.Y >= 0);
            Contract.Requires(size > 0);

            SourcePath = sourcePath;
            Position = position;
            Size = size;
            Channel = channel;
        }

        #endregion

        #region Properties and Fields

        static readonly ICache bmpCache = new ASPNETCache(TimeSpan.FromMinutes(10), CacheItemPriority.BelowNormal);

        public string SourcePath { get; private set; }

        public int Size { get; private set; }

        public Point Position { get; private set; }

        public Channel Channel { get; private set; }

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
            var bmp = GetBitmap(SourcePath);
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

        private static ImageBuffer GetBitmap(string path)
        {
            string key = "BMP:" + path;
            var bmp = bmpCache[key] as ImageBuffer;
            if (bmp == null)
            {
                bmp = new ImageBuffer(new Bitmap(path));
                bmpCache.Add(key, bmp);
            }
            return bmp;
        }

        #endregion
    }
}
