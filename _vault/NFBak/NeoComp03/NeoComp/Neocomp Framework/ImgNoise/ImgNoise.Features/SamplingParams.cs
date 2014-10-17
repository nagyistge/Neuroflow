using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Diagnostics.Contracts;

namespace ImgNoise.Features
{
    public struct SamplingParams
    {
        #region Constructor

        public SamplingParams(int size, int frequency)
        {
            Contract.Requires(size > 0 && size % 2 == 1);
            Contract.Requires(frequency > 0);

            this.size = size;
            this.frequency = frequency;
            this.width = size;
        }

        public SamplingParams(int size, int width, int frequency)
        {
            Contract.Requires(size > 0 && size % 2 == 1);
            Contract.Requires(width > (size / 2) * 2);
            Contract.Requires(frequency > 0);

            this.size = size;
            this.frequency = frequency;
            this.width = width;
        }

        #endregion

        #region Properties

        public bool IsEmpty
        {
            [Pure]
            get
            {
                return size == 0 || width == 0 || frequency == 0;
            }
        }

        int size;

        public int Size
        {
            get { return size; }
        }

        int width;

        public int Width
        {
            get { return width; }
        }

        int frequency;

        public int Frequency
        {
            get { return frequency; }
        } 

        #endregion
    }
}
