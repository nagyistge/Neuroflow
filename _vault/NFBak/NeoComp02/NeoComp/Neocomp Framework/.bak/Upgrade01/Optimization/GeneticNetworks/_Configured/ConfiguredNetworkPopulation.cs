using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks;
using NeoComp.Optimization.GA;

namespace NeoComp.Optimization.GeneticNetworks
{
    public class ConfiguredNetworkPopulation<TGeneData, TNetwork> : Population<NaturalDNA<ConfiguredNetworkGene<TGeneData>>, ConfiguredNetworkBody<TGeneData, TNetwork>>
        where TGeneData : struct
        where TNetwork : class, INetwork
    {
    }
}
