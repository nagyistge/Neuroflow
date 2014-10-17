using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks.Logical;
using NeoComp.Optimization.GA;
using NeoComp.Core;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.GeneticNetworks
{
    public class ConfiguredLogicalNetworkBodyFactory : ConfiguredNetworkBodyFactory<ConfiguredLogicalNetworkParameters, LogicGateType, LogicalNetwork>
    {
        #region Contructor

        public ConfiguredLogicalNetworkBodyFactory(ConfiguredLogicalNetworkParameters parameters)
            : base(parameters)
        {
            Contract.Requires(parameters != null);
        }

        #endregion

        #region Implementation

        protected override LogicGateType CreateAnotherGeneData(LogicGateType geneData)
        {
            var gates = Parameters.AvailableGateArray;
            if (gates.Length == 1) return geneData;
            var another = CreateRandomGeneData();
            while (another == geneData) another = CreateRandomGeneData();
            return another;
        }

        protected override LogicGateType CreateRandomGeneData()
        {
            var gates = Parameters.AvailableGateArray;
            return gates.Length == 1 ? gates[0] : gates[RandomGenerator.Random.Next(gates.Length)];
        }

        protected override ConfiguredNetworkBody<LogicGateType, LogicalNetwork> CreateBody(NaturalDNA<ConfiguredNetworkGene<LogicGateType>> plan)
        {
            return new ConfiguredLogicalNetworkBody(plan, Parameters);
        } 

        #endregion
    }
}
