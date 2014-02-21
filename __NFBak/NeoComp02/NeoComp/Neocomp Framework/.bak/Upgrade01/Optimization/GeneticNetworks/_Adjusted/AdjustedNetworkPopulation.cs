using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks;
using NeoComp.Optimization.GA;

namespace NeoComp.Optimization.GeneticNetworks
{
    public class AdjustedNetworkPopulation<TNetwork> : Population<DNASequence<double>, AdjustedNetworkBody<TNetwork>>
        where TNetwork : class, INetwork
    {
    }
}
