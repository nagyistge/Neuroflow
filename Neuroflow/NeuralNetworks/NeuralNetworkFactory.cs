using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.NeuralNetworks
{
    public sealed class NeuralNetworkFactory
    {
        internal NeuralNetworkFactory(IMultilayerPerceptronAdapter multiplayerPerceptronAdapter)
        {
            Debug.Assert(multiplayerPerceptronAdapter != null);

            this.multiplayerPerceptronAdapter = multiplayerPerceptronAdapter;
        }

        IMultilayerPerceptronAdapter multiplayerPerceptronAdapter;

        public MultilayerPerceptron CreateMultilayerPerceptron(ICollection<Layer> layers, MultilayerPerceptronProperties properties = null)
        {
            return CreateMultilayerPerceptron(new IndexedLayerCollection(layers), properties);
        }

        public MultilayerPerceptron CreateMultilayerPerceptron(IndexedLayerCollection layers, MultilayerPerceptronProperties properties = null)
        {
            Args.Requires(() => layers, () => layers != null && layers.Count > 1);

            ValidateInputLayer(layers[0]);
            ValidateLayers(layers.Skip(1).Take(layers.Count - 2));
            ValidateOutputLayer(layers[layers.Count - 1]);

            return new MultilayerPerceptron(multiplayerPerceptronAdapter, layers, properties ?? new MultilayerPerceptronProperties());
        }

        #region Validation

        private void ValidateLayers(IEnumerable<IndexedLayer> layers)
        {
            foreach (var layer in layers) ValidateLayer(layer);
        }

        private void ValidateLayer(IndexedLayer layer)
        {
            ValidateIfHasInputs(layer);
            ValidateIfHasOutput(layer);
        }

        private void ValidateInputLayer(IndexedLayer layer)
        {
            ValidateIfHasNotInputs(layer);
            ValidateIfHasOutput(layer);
        }

        private void ValidateOutputLayer(IndexedLayer layer)
        {
            ValidateIfHasInputs(layer);
            ValidateIfHasNotOutput(layer);
        }

        private static void ValidateIfHasOutput(IndexedLayer layer)
        {
            if (layer.Layer.OutputConnections.Count == 0) throw new ArgumentException(string.Format("Layer {0} hasn't got output connections.", layer.Index), "layer");
        }

        private static void ValidateIfHasInputs(IndexedLayer layer)
        {
            if (layer.Layer.InputConnections.Count == 0) throw new ArgumentException(string.Format("Layer {0} hasn't got input connections.", layer.Index), "layer");
        }

        private static void ValidateIfHasNotOutput(IndexedLayer layer)
        {
            if (layer.Layer.OutputConnections.Count != 0) throw new ArgumentException(string.Format("Layer {0} has output connections which is not allowed.", layer.Index), "layer");
        }

        private static void ValidateIfHasNotInputs(IndexedLayer layer)
        {
            if (layer.Layer.InputConnections.Count != 0) throw new ArgumentException(string.Format("Layer {0} has input connections which is not allowed.", layer.Index), "layer");
        }

        #endregion
    }
}
