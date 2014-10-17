using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Evolution.GA;
using System.Diagnostics.Contracts;

namespace NeoComp.LogicalEvolution.GA
{
    public class LogicalGAEpoch : GAEpoch<DNA<LogicalNetworkGene>>
    {
        #region Contruct

        public LogicalGAEpoch(LNGAEntityFactory entityFactory, int numberOfPopulations, int populationSize)
            : base(entityFactory, numberOfPopulations, populationSize)
        {
            Contract.Requires(entityFactory != null);
            Contract.Requires(numberOfPopulations > 0);
            Contract.Requires(populationSize > 0);
        }

        #endregion
    }
}
