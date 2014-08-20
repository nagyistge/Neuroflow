using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.NeuralNetworks
{
    public sealed class IndexedLayer
    {
        internal IndexedLayer(Layer layer, int index)
        {
            Debug.Assert(layer != null);
            Debug.Assert(index >= 0);

            Layer = layer;
            Index = index;
        }

        public Layer Layer { get; private set; }

        public int Index { get; private set; }
    }
}
