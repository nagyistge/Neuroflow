using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks;
using NeoComp.Optimization.GA;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.GeneticNetworks
{
    public class AdjustedNetworkGroup<TParameters, TNetwork> : Group<DNASequence<double>, AdjustedNetworkBody<TNetwork>>
        where TParameters : AdjustedNetworkParameters
        where TNetwork : class, INetwork
    {
        public AdjustedNetworkGroup(AdjustedNetworkBodyFactory<TParameters, TNetwork> factory, int size)
            : base(factory, new FactoredBodyTerritory<DNASequence<double>, AdjustedNetworkBody<TNetwork>>(), size)
        {
            Contract.Requires(factory != null);
            Contract.Requires(size > 0);
        }
    }
}
