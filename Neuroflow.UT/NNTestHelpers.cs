using Neuroflow.NeuralNetworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.UT
{
    internal static class NNTestHelpers
    {
        internal static Layer[] CreateGDMLPLayers(bool ff, int inputSize, int hiddenSize, int outputSize, params LayerBehavior[] rules)
        {
            var layers = new[]
            {
                new Layer(inputSize),
                new Layer(hiddenSize)
                {
                    Descriptions =
                    {
                        new ActivationDescription(ActivationFunction.Sigmoid)
                    }
                },
                new Layer(hiddenSize)
                {
                    Descriptions =
                    {
                        new ActivationDescription(ActivationFunction.Sigmoid)
                    }
                },
                new Layer(outputSize)
                {
                    Descriptions =
                    {
                        new ActivationDescription(ActivationFunction.Linear)
                    }
                },
            };

            for (int i = 1; i < layers.Length; i++)
            {
                foreach (var rule in rules) layers[i].Behaviors.Add(rule);
            }

            if (ff)
            {
                layers[0].OutputConnections.AddOneWay(layers[1]);
                layers[1].OutputConnections.AddOneWay(layers[2]);
                layers[2].OutputConnections.AddOneWay(layers[3]);
            }
            else
            {
                layers[0].OutputConnections.AddOneWay(layers[1]);
                layers[1].OutputConnections.AddTwoWay(layers[2]);
                layers[2].OutputConnections.AddTwoWay(layers[3]);
            }

            return layers;
        }

        internal static float Normalize(float value, float min, float max)
        {
            return ((value - min) / (max - min)) * 2.0f - 1.0f;
        }
    }
}
