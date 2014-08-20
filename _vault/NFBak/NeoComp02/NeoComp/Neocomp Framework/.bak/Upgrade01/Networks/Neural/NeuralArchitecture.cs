using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Core;

namespace NeoComp.Networks.Neural
{
    public static class NeuralArchitecture
    {
        public static NeuralNetwork CreateFullConnected(
            int inputInterfaceLength,
            int outputInterfaceLength,
            int neuronCount,
            Func<Synapse> synapseFactoryMethod,
            Func<Neuron> neuronFactoryMethod,
            bool feedForward = true)
        {
            Contract.Requires(inputInterfaceLength > 0);
            Contract.Requires(outputInterfaceLength > 0);
            Contract.Requires(synapseFactoryMethod != null);
            Contract.Requires(neuronFactoryMethod != null);
            Contract.Requires(neuronCount > 0);
            Contract.Requires(feedForward == true); // TODO: Non feed-forward

            var network = new NeuralNetwork(inputInterfaceLength, outputInterfaceLength);
            
            ComputationalArchitecture.InitializeFullConnected(
                network,
                neuronCount,
                synapseFactoryMethod,
                neuronFactoryMethod,
                feedForward);

            return network;
        }

        public static NeuralNetwork CreateLayered(
            int inputInterfaceLength,
            int outputInterfaceLength,
            Func<Synapse> synapseFactoryMethod,
            Func<Neuron> neuronFactoryMethod,
            bool feedForward,
            params int[] neuronCounts)
        {
            Contract.Requires(inputInterfaceLength > 0);
            Contract.Requires(outputInterfaceLength > 0);
            Contract.Requires(synapseFactoryMethod != null);
            Contract.Requires(neuronFactoryMethod != null);
            Contract.Requires(feedForward == true); // TODO: Non feed-forward
            Contract.Requires(!neuronCounts.IsNullOrEmpty());
            Contract.Requires(Contract.ForAll(neuronCounts, (c) => c > 0));

            var network = new NeuralNetwork(inputInterfaceLength, outputInterfaceLength); 
            
            ComputationalArchitecture.InitializeLayered(
                network,
                synapseFactoryMethod,
                neuronFactoryMethod,
                feedForward,
                neuronCounts);

            return network;
        }
    }
}
