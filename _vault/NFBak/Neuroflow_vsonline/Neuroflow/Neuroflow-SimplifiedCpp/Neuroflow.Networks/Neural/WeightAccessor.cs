using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neuroflow.Networks.Neural
{
    public static class WeightAccessor
    {
        public static int GetWeightValueIndex(int upperValueIndex, int lowerValueIndex, int lowerLayerSize)
        {
            return upperValueIndex * lowerLayerSize + lowerValueIndex;
        }
    }
}
