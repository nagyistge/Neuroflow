using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            this.recurrentSampleLength = size;
        }

        public SamplingParams(int size, int recurrentSampleLength, int frequency)
        {
            Contract.Requires(size > 0 && size % 2 == 1);
            Contract.Requires(recurrentSampleLength > (size / 2) * 2);
            Contract.Requires(frequency > 0);

            this.size = size;
            this.frequency = frequency;
            this.recurrentSampleLength = recurrentSampleLength;
        }

        #endregion

        #region Properties

        public bool IsEmpty
        {
            [Pure]
            get
            {
                return size == 0 || recurrentSampleLength == 0 || frequency == 0;
            }
        }

        int size;

        public int Size
        {
            get { return size; }
        }

        int recurrentSampleLength;

        public int RecurrentSampleLength
        {
            get { return recurrentSampleLength; }
        }

        int frequency;

        public int Frequency
        {
            get { return frequency; }
        } 

        #endregion
    }
}
