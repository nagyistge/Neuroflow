using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using AForge.Imaging.Filters;
using System.Drawing.Imaging;

namespace PussyDetector
{
    internal static class ImageHelpers
    {
        internal static unsafe double[,] CreateFeatureArray(Bitmap bmp, bool reversed = false)
        {
            int w = bmp.Width;
            int h = bmp.Height;
            int size = w * h;
            var imageData = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);

            int inc = imageData.Stride >> 2;

            var result = new double[h, w];

            try
            {
                fixed (double* ptrResult = result)
                {
                    double min = double.MaxValue;
                    double max = double.MinValue;

                    for (int y = 0; y < h; y++)
                    {
                        int* start = ((int*)imageData.Scan0) + (y * inc);
                        double* rstart = ptrResult + (y * w);
                        for (int x = 0; x < w; x++)
                        {
                            int* pixel = start + x;

                            uint alpha = (((uint)*pixel & 0xFF000000) >> 24);

                            double dPixel;

                            if (alpha != 0)
                            {
                                dPixel =
                                    ((double)(*pixel & 0xFF) +
                                    (double)((*pixel & 0xFF00) >> 8) +
                                    (double)((*pixel & 0xFF0000) >> 16)) / 3.0;
                            }
                            else
                            {
                                dPixel = double.NaN;
                            }

                            if (reversed)
                            {
                                *(rstart + ((w - 1) - x)) = dPixel;
                            }
                            else
                            {
                                *(rstart + x) = dPixel;
                            }

                            if (alpha != 0)
                            {
                                if (min > dPixel)
                                {
                                    min = dPixel;
                                }
                                else if (max < dPixel)
                                {
                                    max = dPixel;
                                }
                            }
                        }
                    }

                    double d = max - min;

                    for (int i = 0; i < size; i++)
                    {
                        double current = *(ptrResult + i);
                        if (double.IsNaN(current))
                        {
                            current = 0.0;
                        }
                        else
                        {
                            current = ((current - min) / d) * 2.0 - 1.0;
                        }
                        *(ptrResult + i) = current;
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(imageData);
            }

            return result;
        }
        
        internal static Bitmap PreprocessBitmap(string fileName, int sampleSize)
        {
            using (var bmp = new Bitmap(fileName))
            {
                return PreprocessBitmap(bmp, sampleSize);
            }
        }

        internal static Bitmap PreprocessBitmap(Bitmap bmp, int sampleSize)
        {
            double w = bmp.Width;
            double h = bmp.Height;

            var bkgBmp = new Bitmap(sampleSize, sampleSize);
            var g = Graphics.FromImage(bkgBmp);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            var alpha = new SolidBrush(Color.FromArgb(0, 0, 0, 0));
            g.FillRectangle(alpha, 0, 0, sampleSize, sampleSize);

            int d;
            double factor;

            if (w < h)
            {
                factor = (double)sampleSize / h;
                d = (int)Math.Round(((h / 2) - (w / 2)) * factor);
                var rect = new Rectangle(d, 0, sampleSize - d, sampleSize);
                g.DrawImage(bmp, rect);
            }
            else
            {
                factor = (double)sampleSize / w;
                d = (int)Math.Round(((w / 2) - (h / 2)) * factor);
                var rect = new Rectangle(0, d, sampleSize, sampleSize - d);
                g.DrawImage(bmp, rect);
            }

            return bkgBmp;
        }
    }
}
