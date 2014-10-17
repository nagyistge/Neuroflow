using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Web.Caching;
using NeoComp.Caching;
using NeoComp.Imaging;

namespace ImgNoise.Features
{
    public abstract class ImageReader
    {
        #region Constructor

        protected ImageReader(string sourcePath, Point position, int size, Channel channel)
        {
            Contract.Requires(!String.IsNullOrEmpty(sourcePath));
            Contract.Requires(!position.IsEmpty);
            Contract.Requires(position.X >= 0);
            Contract.Requires(position.Y >= 0);
            Contract.Requires(size > 0 && size % 2 == 1);

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

        protected ImageBuffer GetBitmap()
        {
            string key = "BMP:" + SourcePath;
            var bmp = bmpCache[key] as ImageBuffer;
            if (bmp == null)
            {
                bmp = new ImageBuffer(new Bitmap(SourcePath));
                bmpCache.Add(key, bmp);
            }
            return bmp;
        }

        protected byte GetCahnnelValue(Color pixel)
        {
            switch (Channel)
            {
                case Channel.Red:
                    return pixel.R;
                case Channel.Green:
                    return pixel.G;
                default:
                    return pixel.B;
            }
        }
    }
}
