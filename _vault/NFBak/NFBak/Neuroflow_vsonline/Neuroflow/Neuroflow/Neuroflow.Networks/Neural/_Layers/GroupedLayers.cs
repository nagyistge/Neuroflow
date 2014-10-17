using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Diagnostics;

namespace Neuroflow.Networks.Neural
{
    public sealed class GroupedLayers
    {
        public GroupedLayers(ICollection<ConnectableLayer> layers)
        {
            Contract.Requires(layers != null);
            Contract.Requires(layers.Count >= 2);

            LayerGroups = CreateGroups(layers);

            InputLayer = FindInputLayer();
            OutputLayer = FindOutputLayer();
        }

        public ConnectableLayer InputLayer { get; private set; }

        public ConnectableLayer OutputLayer { get; private set; }

        public ReadOnlyCollection<ReadOnlyCollection<ConnectableLayer>> LayerGroups { get; private set; }

        private ReadOnlyCollection<ReadOnlyCollection<ConnectableLayer>> CreateGroups(IEnumerable<ConnectableLayer> layers)
        {
            var cmp = new LayerOrderComparer();
            var ordered = (from l in layers
                           group l by l.UID into g
                           select g.First()).OrderBy(l => l, cmp);

            var current = new List<ConnectableLayer>();
            var currentRO = current.AsReadOnly();

            var groups = new List<ReadOnlyCollection<ConnectableLayer>>();

            ConnectableLayer prevLayer = null;
            foreach (var layer in ordered)
            {
                if (prevLayer != null)
                {
                    if (cmp.Compare(prevLayer, layer) < 0)
                    {
                        current.TrimExcess();
                        groups.Add(currentRO);
                        current = new List<ConnectableLayer>();
                        currentRO = current.AsReadOnly();
                    }
                }
                prevLayer = layer;

                current.Add(layer);
            }

            if (current.Count != 0)
            {
                groups.Add(currentRO);
            }

            if (groups.Count < 2) throw GetConnectionErrorEx();

            groups.TrimExcess();

            return groups.AsReadOnly();
        }

        private ConnectableLayer FindInputLayer()
        {
            Debug.Assert(LayerGroups != null);
            Debug.Assert(LayerGroups.Count >= 2);

            var lg = LayerGroups[0];

            if (lg.Count != 1) throw GetConnectionErrorEx();

            return lg[0];
        }

        private ConnectableLayer FindOutputLayer()
        {
            Debug.Assert(LayerGroups != null);
            Debug.Assert(LayerGroups.Count >= 2);

            var lg = LayerGroups[LayerGroups.Count - 1];

            if (lg.Count != 1) throw GetConnectionErrorEx();

            return lg[0];
        }

        private static Exception GetConnectionErrorEx()
        {
            return new InvalidOperationException("Layer connection error.");
        }
    }
}
