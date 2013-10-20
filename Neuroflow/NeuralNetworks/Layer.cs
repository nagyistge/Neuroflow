using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.NeuralNetworks
{
    public sealed class Layer
    {
        public Layer(int size)
        {
            Args.Requires(() => size, () => size > 0);

            Size = size;
            InputConnections = new LayerConnections(LayerConnections.ConnectionType.Input, this);
            OutputConnections = new LayerConnections(LayerConnections.ConnectionType.Output, this);
            Behaviors = new LayerBehavior.Collection();
            Descriptions = new LayerDescription.Collection();
        }

        public int Size { get; private set; }

        public LayerConnections InputConnections { get; private set; }

        public LayerConnections OutputConnections { get; private set; }

        public ICollection<LayerBehavior> Behaviors { get; private set; }

        public ICollection<LayerDescription> Descriptions { get; private set; }

        public bool HasRecurrentConnections
        {
            get
            {
                return InputConnections.GetConnectedLayers(FlowDirection.TwoWay | FlowDirection.OneWayToSource).Any() ||
                    OutputConnections.GetConnectedLayers(FlowDirection.TwoWay | FlowDirection.OneWayToSource).Any();
            }
        }

        public IEnumerable<Layer> GetInputLayers()
        {
            return InputConnections.GetConnectedLayers(FlowDirection.OneWay | FlowDirection.TwoWay)
                .Concat(OutputConnections.GetConnectedLayers(FlowDirection.TwoWay | FlowDirection.OneWayToSource));
        }

        public IEnumerable<Layer> GetOutputLayers()
        {
            return OutputConnections.GetConnectedLayers(FlowDirection.OneWay | FlowDirection.TwoWay)
                .Concat(InputConnections.GetConnectedLayers(FlowDirection.TwoWay | FlowDirection.OneWayToSource));
        }

        public Layer GetInputLayer(int connectionIndex)
        {
            var q = from item in GetInputLayers().Select((l, idx) => new { l, idx })
                    where item.idx == connectionIndex
                    select item.l;

            var layer = q.SingleOrDefault();

            if (layer == null) throw CreateLayerNotFoundException("Input", connectionIndex);

            return layer;
        }

        public Layer GetOutputLayer(int connectionIndex)
        {
            var q = from item in GetOutputLayers().Select((l, idx) => new { l, idx })
                    where item.idx == connectionIndex
                    select item.l;

            var layer = q.SingleOrDefault();

            if (layer == null) throw CreateLayerNotFoundException("Output", connectionIndex);

            return layer;
        }

        private Exception CreateLayerNotFoundException(string type, int connectionIndex)
        {
            return new ArgumentOutOfRangeException(type + " layer not found, connection index value " + connectionIndex + " was out of range.", "connectionIndex");
        }
    }
}
