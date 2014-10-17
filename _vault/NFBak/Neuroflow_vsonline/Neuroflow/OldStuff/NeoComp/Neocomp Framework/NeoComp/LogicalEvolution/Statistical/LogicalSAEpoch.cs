using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Evolution.Statistical;
using System.Diagnostics.Contracts;

namespace NeoComp.LogicalEvolution.Statistical
{
    public class LogicalSAEpoch : StatisticalEvolutionEpoch<LogicalNetworEntity>
    {
        #region Contruct

        public LogicalSAEpoch(LNStatisticalEntityFactory entityFactory, int numberOfPopulations, int populationSize, int numberOfElites)
            : base(entityFactory, numberOfPopulations, populationSize, numberOfElites)
        {
            Contract.Requires(numberOfPopulations > 0);
            Contract.Requires(entityFactory != null);
            Contract.Requires(populationSize > 0);
            Contract.Requires(numberOfElites > 0 && numberOfElites < populationSize);
        }

        #endregion
    }
}
