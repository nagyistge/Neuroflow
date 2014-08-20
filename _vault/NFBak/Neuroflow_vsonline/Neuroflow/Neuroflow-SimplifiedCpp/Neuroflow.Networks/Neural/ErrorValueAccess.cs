using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Neuroflow.Core;

namespace Neuroflow.Networks.Neural
{
    public struct ErrorValueAccess
    {
        public ErrorValueAccess(int errorSize, int errorBufferBeginIndex, int weightBufferBeginIndex)
        {
            Contract.Requires(errorSize > 0);
            Contract.Requires(errorBufferBeginIndex >= 0);
            Contract.Requires(weightBufferBeginIndex >= 0);

            this.errorSize = errorSize;
            this.errorBufferBeginIndex = errorBufferBeginIndex;
            this.weightBufferBeginIndex = weightBufferBeginIndex;
        }

        int errorSize;

        public int ErrorSize
        {
            get { return errorSize; }
        }

        int errorBufferBeginIndex;

        public int ErrorBufferBeginIndex
        {
            get { return errorBufferBeginIndex; }
        }

        int weightBufferBeginIndex;

        public int WeightBufferBeginIndex
        {
            get { return weightBufferBeginIndex; }
        }
    }
}
