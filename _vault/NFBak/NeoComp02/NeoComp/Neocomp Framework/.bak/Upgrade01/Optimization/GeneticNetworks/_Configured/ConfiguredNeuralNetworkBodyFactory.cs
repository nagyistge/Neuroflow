using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using NeoComp.Optimization.GA;
using NeoComp.Networks.Neural;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.GeneticNetworks
{
    public class ConfiguredNeuralNetworkBodyFactory : ConfiguredNetworkBodyFactory<ConfiguredNeuralNetworkParameters, ConfiguredNeuralGeneData, NeuralNetwork>
    {
        #region Contructor

        public ConfiguredNeuralNetworkBodyFactory(ConfiguredNeuralNetworkParameters parameters)
            : base(parameters)
        {
            Contract.Requires(parameters != null);
        }

        #endregion

        #region Implementation

        protected override ConfiguredNeuralGeneData CreateAnotherGeneData(ConfiguredNeuralGeneData geneData)
        {
            if (RandomGenerator.FiftyPercentChance)
            {
                return new ConfiguredNeuralGeneData(CreateRandomAdjustment(), geneData.Bias);
            }
            else
            {
                return new ConfiguredNeuralGeneData(geneData.Weight, CreateRandomAdjustment());
            }
        }

        protected override ConfiguredNeuralGeneData CreateRandomGeneData()
        {
            return new ConfiguredNeuralGeneData(CreateRandomAdjustment(), CreateRandomAdjustment());
        }

        private double CreateAnotherAdjustment(double adjustment)
        {
            //return RandomGenerator.NextDouble(-1.0, 1.0);
            adjustment += (RandomGenerator.Random.NextDouble() * 2.0 - 1.0) / 10.0;
            if (adjustment < -1.0) adjustment = -1.0; else if (adjustment > 1.0) adjustment = 1.0;
            return adjustment;
        } 

        private double CreateRandomAdjustment()
        {
            return RandomGenerator.NextDouble(-1.0, 1.0);
        }

        protected override ConfiguredNetworkBody<ConfiguredNeuralGeneData, NeuralNetwork> CreateBody(NaturalDNA<ConfiguredNetworkGene<ConfiguredNeuralGeneData>> plan)
        {
            return new ConfiguredNeuralNetworkBody(plan, Parameters);
        }

        #endregion
    }
}
