using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.NeuralNetworks
{
    public sealed class IndexedLayerCollection : ReadOnlyCollection<IndexedLayer>
    {
        public IndexedLayerCollection(ICollection<Layer> layers) :
            base(MakeIndexed(layers))
        {
        }

        private static IndexedLayer[] MakeIndexed(ICollection<Layer> layers)
        {
            Args.Requires(() => layers, () => layers != null && layers.Count > 0);

            return layers.OrderBy(l => l, new LayerOrderComparer())
                         .Select((l, idx) => new IndexedLayer(l, idx))
                         .ToArray();
        }
    }
}
