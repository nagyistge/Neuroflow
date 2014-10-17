using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics.Contracts;
using System.Drawing;

namespace PussyDetector
{
    public sealed class FeatureStream
    {
        public FeatureStream(FileInfo fi, bool isObjectDetected, bool reversed = false)
        {
            Contract.Requires(fi != null);

            this.fi = fi;
            IsObjectDetected = isObjectDetected;
            this.reversed = reversed;
        }

        FileInfo fi;

        bool reversed;

        public bool IsObjectDetected { get; private set; }

        public Bitmap CreatePreprocessedBitmap(int sampleSize)
        {
            Contract.Requires(sampleSize > 0);

            return ImageHelpers.PreprocessBitmap(fi.FullName, sampleSize);
        }

        public double[,] CreateFeatureArray(int sampleSize)
        {
            Contract.Requires(sampleSize > 0);

            using (var bmp = CreatePreprocessedBitmap(sampleSize))
            {
                return ImageHelpers.CreateFeatureArray(bmp, reversed);
            }
        }

        public FeatureStream GetReversed()
        {
            return new FeatureStream(fi, IsObjectDetected, !reversed);
        }
    }
}
