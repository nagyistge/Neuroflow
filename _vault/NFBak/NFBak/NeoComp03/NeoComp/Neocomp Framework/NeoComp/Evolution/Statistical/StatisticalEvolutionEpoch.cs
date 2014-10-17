using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Epoch;
using NeoComp.Core;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace NeoComp.Evolution.Statistical
{
    public class StatisticalEvolutionEpoch<T> : EpochBase
        where T : class, IComparable<T>
    {
        #region Contruct

        public StatisticalEvolutionEpoch(IStatisticalEntityFactory<T> entityFactory, int numberOfPopulations, int populationSize, int numberOfElites)
        {
            Contract.Requires(numberOfPopulations > 0);
            Contract.Requires(entityFactory != null);
            Contract.Requires(populationSize > 0);
            Contract.Requires(numberOfElites > 0 && numberOfElites < populationSize);

            Factory = entityFactory;
            NumberOfPopulations = numberOfPopulations;
            PopulationSize = populationSize;
            NumberOfElites = numberOfElites;
        }

        #endregion

        #region Properties and Fields

        Population[] populations;

        public IStatisticalEntityFactory<T> Factory { get; private set; }

        public int NumberOfPopulations { get; private set; }

        public int PopulationSize { get; private set; }

        public int NumberOfElites { get; private set; }

        T bestEntity;

        public T BestEntity
        {
            get { lock (SyncRoot) return bestEntity; }
        }

        #endregion

        #region Init

        protected override void DoInitialize()
        {
            base.DoInitialize();

            if (populations == null)
            {
                populations = new Population[NumberOfPopulations];
                for (int idx = 0; idx < populations.Length; idx++)
                {
                    var pop = populations[idx] = new Population(Factory.Units.Count);
                    pop.Initialize(Factory.Units);
                }
            }
            else
            {
                for (int idx = 0; idx < populations.Length; idx++)
                {
                    populations[idx].Initialize(Factory.Units);
                }
            }
        }

        protected override void DoUninitialize()
        {
            base.DoUninitialize();

            populations = null;
        }

        #endregion

        #region Entropy Impl

        protected override void DoStep()
        {
#if DEBUG
            foreach (var pop in populations) StepPopulation(pop);
#else
            foreach (var pop in populations) StepPopulation(pop);
            //Parallel.ForEach(populations, StepPopulation);
#endif
        }

        private void StepPopulation(Population population)
        {
            var entities = new SortedList<T, EntityDataUnit[]>();
            for (int entityIndex = 0; entityIndex < PopulationSize; entityIndex++)
            {
                var entityDataUnits = GetEntityDataUnits(population);
                if (populations.Length > 1 && new Probability(0.01))
                {
                    entityDataUnits = Crossover(entityDataUnits, population);
                }
                // Add entity to list:
                entities.Add(Factory.CreateEntity(entityDataUnits), entityDataUnits);
            }

            RegisterBestEntity(entities.Keys[0]);

            // Compute Entropy and Register:
            var eliteDataUnits = new EntityDataUnit[NumberOfElites];
            for (int unitIndex = 0; unitIndex < Factory.Units.Count; unitIndex++)
            {
                var currentUnit = Factory.Units[unitIndex];

                for (int eliteIndex = 0; eliteIndex < NumberOfElites; eliteIndex++)
                {
                    eliteDataUnits[eliteIndex] = entities.Values[eliteIndex][unitIndex];
                }

                object newContext = currentUnit.ComputeNewContext(eliteDataUnits);
                population.UnitContexts[unitIndex] = newContext;
            }
        }

        private EntityDataUnit[] Crossover(EntityDataUnit[] entityDataUnits, Population ownerPopulation)
        {
            if (entityDataUnits.Length > 4)
            {
                int opidx = RandomGenerator.Random.Next(populations.Length);
                var otherPop = populations[opidx];
                while (otherPop == ownerPopulation)
                {
                    opidx = RandomGenerator.Random.Next(populations.Length);
                    otherPop = populations[opidx];
                }
                var otherUnits = GetEntityDataUnits(otherPop);
                if (otherUnits.Length > 4)
                {
                    var genom = new[] { entityDataUnits, otherUnits };
                    var genes = genom.Crossover(IntRange.CreateInclusive(1, entityDataUnits.Length / 2)).ToArray();
                    return genes;
                }
            }
            return entityDataUnits;
        }

        private EntityDataUnit[] GetEntityDataUnits(Population population)
        {
            var entityDataUnits = new EntityDataUnit[Factory.Units.Count];
            for (int unitIndex = 0; unitIndex < Factory.Units.Count; unitIndex++)
            {
                var currentUnit = Factory.Units[unitIndex];
                var currentContext = population.UnitContexts[unitIndex];
                if (new Probability(0.02))
                {
                    entityDataUnits[unitIndex] = currentUnit.CreateRandomEntityDataUnit();
                }
                else
                {
                    entityDataUnits[unitIndex] = currentUnit.CreateEntityDataUnit(currentContext);
                }
            }
            return entityDataUnits;
        }

        private void RegisterBestEntity(T entity)
        {
            lock (SyncRoot)
            {
                if (bestEntity == null || entity.CompareTo(bestEntity) < 0) bestEntity = entity;
            }
        }

        #endregion
    }
}
