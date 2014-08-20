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

        public SamplingParams(int size, int frequency = 10)
        {
            Contract.Requires(size > 0);
            Contract.Requires(frequency > 0);

            this.size = size;
            this.frequency = frequency;
        }

        #endregion

        #region Properties

        public bool IsEmpty
        {
            [Pure]
            get
            {
                return size == 0 || frequency == 0;
            }
        }

        int size;

        public int Size
        {
            get { return size; }
        }

        int frequency;

        public int Frequency
        {
            get { return frequency; }
        } 

        #endregion
    }
}
