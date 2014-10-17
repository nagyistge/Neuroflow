
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Neuroflow.Networks.Neural
{
    public struct UpperLayerInfo
    {
        public UpperLayerInfo(int weightedErrorBufferIndex, int layerIndex)
        {
            Contract.Requires(weightedErrorBufferIndex >= 0);
            Contract.Requires(layerIndex >= 0);

            this.weightedErrorBufferIndex = weightedErrorBufferIndex;
            this.layerIndex = layerIndex;
        }
        
        private int weightedErrorBufferIndex;

        public int WeightedErrorBufferIndex
        {
            get { return weightedErrorBufferIndex; }
            private set { weightedErrorBufferIndex = value; }
        }

        private int layerIndex;

        public int LayerIndex
        {
            get { return layerIndex; }
            private set { layerIndex = value; }
        }
    }
}
