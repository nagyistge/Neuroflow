using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks.Neural;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.GeneticNetworks
{
    public class ConfiguredNeuralNetworkGroup : ConfiguredNetworkGroup<ConfiguredNeuralNetworkParameters, ConfiguredNeuralGeneData, NeuralNetwork>
    {
        public ConfiguredNeuralNetworkGroup(ConfiguredNeuralNetworkBodyFactory factory, int size)
            : base(factory, size)
        {
            Contract.Requires(factory != null);
            Contract.Requires(size > 0);
        }
    }
}
