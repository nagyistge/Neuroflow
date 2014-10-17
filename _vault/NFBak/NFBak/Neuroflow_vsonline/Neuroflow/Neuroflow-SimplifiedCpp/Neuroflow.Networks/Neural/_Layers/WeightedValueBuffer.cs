using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neuroflow.Core;
using System.Diagnostics.Contracts;

namespace Neuroflow.Networks.Neural
{
    public struct WeightedValueBuffer
    {
        public WeightedValueBuffer(IntRange valueBuffer, IntRange weightBuffer)
        {
            this.valueBuffer = valueBuffer;
            this.weightBuffer = weightBuffer;
        }

        IntRange valueBuffer;

        public IntRange ValueBuffer
        {
            get { return valueBuffer; }
        }

        IntRange weightBuffer;

        public IntRange WeightBuffer
        {
            get { return weightBuffer; }
        }
    }
}
