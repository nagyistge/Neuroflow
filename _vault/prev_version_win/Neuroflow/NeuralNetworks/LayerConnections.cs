using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.NeuralNetworks
{
    public sealed class LayerConnections
    {
        internal enum ConnectionType { Input, Output }

        internal LayerConnections(ConnectionType type, Layer connectedLayer)
        {
            Debug.Assert(connectedLayer != null);

            ConnectedLayer = connectedLayer;
            Type = type;
        }

        List<Tuple<FlowDirection, Layer>> otherLayers = new List<Tuple<FlowDirection, Layer>>();

        bool suppressOtherSideUpdate;

        internal ConnectionType Type { get; private set; }

        internal Layer ConnectedLayer { get; private set; }

        public int Count
        {
            get { return otherLayers.Count; }
        }

        public IEnumerable<Layer> GetConnectedLayers(FlowDirection direction)
        {
            return otherLayers.Where(i => (int)(i.Item1 & direction) != 0).Select(i => i.Item2);
        }

        public void AddOneWay(Layer item)
        {
            Add(item, FlowDirection.OneWay);
        }

        public void AddTwoWay(Layer item)
        {
            Add(item, FlowDirection.TwoWay);
        }

        public void AddOneWayToSource(Layer item)
        {
            Add(item, FlowDirection.OneWayToSource);
        }

        public void Add(Layer item, FlowDirection direction)
        {
            Args.Requires(() => item, () => item != null);
            Args.Requires(() => direction, () => direction == FlowDirection.OneWay || direction == FlowDirection.OneWayToSource || direction == FlowDirection.TwoWay);

            if (!otherLayers.Any(l => l.Item2 == item))
            {
                WithOtherSideUpdateSuppressed(item, () =>
                {
                    if (Type == ConnectionType.Output)
                    {
                        item.InputConnections.Add(ConnectedLayer, direction);
                    }
                    else
                    {
                        item.OutputConnections.Add(ConnectedLayer, direction);
                    }
                });
                otherLayers.Add(Tuple.Create(direction, item));
            }
        }

        public bool Remove(Layer item)
        {
            Args.Requires(() => item, () => item != null);

            var toRemove = otherLayers.FirstOrDefault(i => i.Item2 == item);
            if (toRemove != null && otherLayers.Remove(toRemove))
            {
                WithOtherSideUpdateSuppressed(item, () =>
                {
                    if (Type == ConnectionType.Output)
                    {
                        item.InputConnections.Remove(ConnectedLayer);
                    }
                    else
                    {
                        item.OutputConnections.Remove(ConnectedLayer);
                    }
                });
                return true;
            }
            return false;
        }

        public void Clear()
        {
            foreach (var layer in otherLayers.Select(i => i.Item2))
            {
                if (Type == ConnectionType.Output)
                {
                    layer.InputConnections.Remove(ConnectedLayer);
                }
                else
                {
                    layer.OutputConnections.Remove(ConnectedLayer);
                }
            }
            otherLayers.Clear();
        }

        private void WithOtherSideUpdateSuppressed(Layer item, Action method)
        {
            if (!suppressOtherSideUpdate)
            {
                ((LayerConnections)item.InputConnections).suppressOtherSideUpdate = true;
                ((LayerConnections)item.OutputConnections).suppressOtherSideUpdate = true;
                try
                {
                    method();
                }
                finally
                {
                    ((LayerConnections)item.InputConnections).suppressOtherSideUpdate = false;
                    ((LayerConnections)item.OutputConnections).suppressOtherSideUpdate = false;
                }
            }
        }
    }
}
