using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.IO;

namespace Neuroflow.Core.Imaging
{
    public class ImageBuffer
    {
        #region Constructors

        public ImageBuffer(int width, int height)
        {
            Contract.Requires(width > 0);
            Contract.Requires(height > 0);

            Data = new int[width * height];
            Width = width;
            Height = height;
        }

        public ImageBuffer(Bitmap bmp)
        {
            Contract.Requires(bmp != null);

            BitmapData bits = null;
            try
            {
                Width = bmp.Width;
                Height = bmp.Height;
                Data = new int[Width * Height];
                bits = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                var ptr = bits.Scan0;
                for (int y = 0; y < Height; y++, ptr += bits.Stride)
                {
                    Marshal.Copy(ptr, Data, y * Width, Width);
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Cannot create from bitmap with Pixel Format '" + bmp.PixelFormat + "'.", "bmp", ex);
            }
            finally
            {
                if (bits != null) bmp.UnlockBits(bits);
            }
        }

        #endregion

        #region Properties

        public int[] Data { get; private set; }

        public int Width { get; private set; }

        public int Height { get; private set; }

        #endregion

        #region Bitmap

        public Bitmap GetBitmap()
        {
            var bmp = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
            SafeWriteTo(bmp);
            return bmp;
        }

        public void WriteTo(Bitmap bmp)
        {
            Contract.Requires(bmp != null);
            Contract.Requires(bmp.Width == Width);
            Contract.Requires(bmp.Height == Height);

            SafeWriteTo(bmp);
        }

        private void SafeWriteTo(Bitmap bmp)
        {
            BitmapData bits = null;
            try
            {
                bits = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                var ptr = bits.Scan0;
                for (int y = 0; y < Height; y++, ptr += bits.Stride)
                {
                    Marshal.Copy(Data, y * Width, ptr, Width);
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Cannot write to bitmap with Pixel Format '" + bmp.PixelFormat + "'.", "bmp", ex);
            }
            finally
            {
                if (bits != null) bmp.UnlockBits(bits);
            }
        }

        #endregion

        #region Save

        public void Save(string filename)
        {
            Contract.Requires(!string.IsNullOrEmpty(filename));

            using (var bmp = GetBitmap())
            {
                bmp.Save(filename);
            }
        }

        public void Save(Stream stream, ImageFormat format)
        {
            Contract.Requires(stream != null);
            Contract.Requires(format != null);

            using (var bmp = GetBitmap())
            {
                bmp.Save(stream, format);
            }
        }

        public void Save(string filename, ImageFormat format)
        {
            Contract.Requires(!string.IsNullOrEmpty(filename));
            Contract.Requires(format != null);

            using (var bmp = GetBitmap())
            {
                bmp.Save(filename, format);
            }
        }

        public void Save(Stream stream, ImageCodecInfo encoder, EncoderParameters encoderParams)
        {
            Contract.Requires(stream != null);
            Contract.Requires(encoder != null);
            Contract.Requires(encoderParams != null);

            using (var bmp = GetBitmap())
            {
                bmp.Save(stream, encoder, encoderParams);
            }
        }

        public void Save(string filename, ImageCodecInfo encoder, EncoderParameters encoderParams)
        {
            Contract.Requires(!string.IsNullOrEmpty(filename));
            Contract.Requires(encoder != null);
            Contract.Requires(encoderParams != null);

            using (var bmp = GetBitmap())
            {
                bmp.Save(filename, encoder, encoderParams);
            }
        }

        #endregion

        #region Pixel

        public Color GetPixel(int x, int y)
        {
            Contract.Requires(x >= 0 && x < Width);
            Contract.Requires(y >= 0 && y < Height);

            return SafeGetPixel(x, y);
        }

        public Color SafeGetPixel(int x, int y)
        {
            return Color.FromArgb(Data[GetPos(x, y)]);
        }

        public void SetPixel(int x, int y, Color color)
        {
            Contract.Requires(x >= 0 && x < Width);
            Contract.Requires(y >= 0 && y < Height);

            SafeSetPixel(x, y, color);
        }

        public void SafeSetPixel(int x, int y, Color color)
        {
            Data[GetPos(x, y)] = ToInt(color);
        }

        #endregion

        #region Stuff

        private int GetPos(int x, int y)
        {
            return y * Width + x;
        }

        private static int ToInt(Color color)
        {
            return (color.A << 24) + (color.R << 16) + (color.G << 8) + color.B;
        }

        #endregion
    }
}
