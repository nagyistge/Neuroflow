using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Neuroflow.Core.ComponentModel;

namespace Neuroflow.Networks.Neural
{
    public class StandardMultilayerArchitecture : NeuralNetworkArchitecture
    {
        [Required]
        [Category(PropertyCategories.Structure)]
        [DisplayName("Input Size")]
        public int InputSize { get; set; }

        [Category(PropertyCategories.Structure)]
        [DisplayName("Hidden Layers")]
        public IList<Layer> HiddenLayers { get; set; }

        [Required]
        [Category(PropertyCategories.Structure)]
        [DisplayName("Output Layer")]
        public Layer OutputLayer { get; set; }

        protected override void Validate()
        {
            base.Validate();
            if (InputSize <= 0) throw new InvalidOperationException("InputInterfaceLength must be greater than zero.");
            if (OutputLayer == null) throw new InvalidOperationException("OutputLayer is null.");
        }

        protected override ICollection<ConnectableLayer> CreateLayers()
        {
            int hiddenLayersCount = HiddenLayers != null ? HiddenLayers.Count : 0;
            var layers = new List<ConnectableLayer>(1 + hiddenLayersCount + 1);

            var inputLayer = new InputLayer(InputSize).Connectable();
            layers.Add(inputLayer);

            var prevLayer = inputLayer;

            if (hiddenLayersCount != 0)
            {
                foreach (var hl in HiddenLayers)
                {
                    var hiddenLayer = hl.Connectable();
                    prevLayer.LowerLayers.Add(hiddenLayer);
                    layers.Add(hiddenLayer);
                    prevLayer = hiddenLayer;
                }
            }

            var outputLayer = OutputLayer.Connectable();
            prevLayer.LowerLayers.Add(outputLayer);
            layers.Add(outputLayer);

            return layers;
        }
    }
}
