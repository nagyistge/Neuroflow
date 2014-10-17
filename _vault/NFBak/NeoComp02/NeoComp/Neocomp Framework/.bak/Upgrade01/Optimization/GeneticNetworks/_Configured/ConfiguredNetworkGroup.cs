using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Optimization.GA;
using NeoComp.Networks;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.GeneticNetworks
{
    public class ConfiguredNetworkGroup<TParameters, TGeneData, TNetwork> : Group<NaturalDNA<ConfiguredNetworkGene<TGeneData>>, ConfiguredNetworkBody<TGeneData, TNetwork>>
        where TParameters : ConfiguredNetworkParameters
        where TGeneData : struct
        where TNetwork : class, INetwork
    {
        public ConfiguredNetworkGroup(ConfiguredNetworkBodyFactory<TParameters, TGeneData, TNetwork> factory, int size)
            : base(factory, new FactoredBodyTerritory<NaturalDNA<ConfiguredNetworkGene<TGeneData>>, ConfiguredNetworkBody<TGeneData, TNetwork>>(), size)
        {
            Contract.Requires(factory != null);
            Contract.Requires(size > 0);
        }
    }
}
