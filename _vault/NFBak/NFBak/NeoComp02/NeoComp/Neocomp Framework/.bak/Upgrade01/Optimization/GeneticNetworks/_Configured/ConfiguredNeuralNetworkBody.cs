using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks.Neural;
using NeoComp.Optimization.GA;
using NeoComp.Networks;
using NeoComp.Core;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.GeneticNetworks
{
    public class ConfiguredNeuralNetworkBody : ConfiguredTestableNetworkBody<ConfiguredNeuralGeneData, NeuralNetwork>
    {
        #region Constructors

        public ConfiguredNeuralNetworkBody(NaturalDNA<ConfiguredNetworkGene<ConfiguredNeuralGeneData>> dna, ConfiguredNeuralNetworkParameters parameters)
            : base(dna, parameters)
        {
            Contract.Requires(dna != null);
            Contract.Requires(parameters != null);
        }

        public ConfiguredNeuralNetworkBody(Guid uid, NaturalDNA<ConfiguredNetworkGene<ConfiguredNeuralGeneData>> dna, ConfiguredNeuralNetworkParameters parameters)
            : base(uid, dna, parameters)
        {
            Contract.Requires(dna != null);
            Contract.Requires(parameters != null);
        }

        #endregion

        #region Properties

        new protected ConfiguredNeuralNetworkParameters Parameters
        {
            get { return (ConfiguredNeuralNetworkParameters)base.Parameters; }
        }

        #endregion

        #region Implementation

        protected override NeuralNetwork CreateNetworkInstance()
        {
            return new NeuralNetwork(Parameters.InputInterfaceLength, Parameters.OutputInterfaceLength);
        }

        protected override Connection CreateConnection(ConfiguredNetworkGene<ConfiguredNeuralGeneData> fromGene)
        {
            return new Synapse(fromGene.Data.Weight);
        }

        protected override Node CreateNode(ConfiguredNetworkGene<ConfiguredNeuralGeneData> fromGene)
        {
            return new ActivationNeuron(fromGene.Data.Bias, Parameters.ActivationFunction);
        }

        #endregion
    }
}
