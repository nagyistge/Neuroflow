using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Epoch;
using NeoComp.Core;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using NeoComp.Optimization.Algorithms.Selection;

namespace NeoComp.Evolution.GA
{
    public class GAEpoch<TPlan> : IEpoch
        where TPlan : class
    {
        #region Contruct

        public GAEpoch(IGAEntityFactory<TPlan> entityFactory, int numberOfPopulations, int populationSize)
        {
            Contract.Requires(entityFactory != null);
            Contract.Requires(numberOfPopulations > 0);
            Contract.Requires(populationSize > 0);

            EntityFactory = entityFactory;
            NumberOfPopulations = numberOfPopulations;
            PopulationSize = populationSize;
        }

        #endregion

        #region GA Props and Fields

        Population<TPlan>[] populations;

        public int NumberOfPopulations { get; private set; }

        public int PopulationSize { get; private set; }

        IntRange numberOfParentsRange = IntRange.CreateFixed(2);

        public IntRange NumberOfParentsRange
        {
            get { lock (SyncRoot) return numberOfParentsRange; }
            set
            {
                Contract.Requires(value.MinValue > 0);

                lock (SyncRoot) numberOfParentsRange = value;
            }
        }

        public IGAEntityFactory<TPlan> EntityFactory { get; private set; }

        Entity<TPlan> bestEntity;

        public Entity<TPlan> BestEntity
        {
            get { lock (SyncRoot) return bestEntity; }
        }

        double bestSelectStdDev = 0.2;

        public double BestSelectStdDev
        {
            get { lock (SyncRoot) return bestSelectStdDev; }
            set
            {
                Contract.Requires(value > 0.0);

                lock (SyncRoot) bestSelectStdDev = value;
            }
        }

        double worstSelectStdDev = 0.01;

        public double WorstSelectStdDev
        {
            get { lock (SyncRoot) return worstSelectStdDev; }
            set
            {
                Contract.Requires(value > 0.0);

                lock (SyncRoot) worstSelectStdDev = value;
            }
        }

        Probability offspringMovingChance = 0.1;

        public Probability OffspringMovingChance
        {
            get { lock (SyncRoot) return offspringMovingChance; }
            set { lock (SyncRoot) offspringMovingChance = value; }
        }

        #endregion

        #region Epoch Impl

        SyncContext syncRoot = new SyncContext();

        public SyncContext SyncRoot
        {
            get { return syncRoot; }
        }

        int currentIteration;

        public int CurrentIteration
        {
            get { lock (SyncRoot) return currentIteration; }
        }

        bool initialized;

        public bool Initialized
        {
            get { lock (SyncRoot) return initialized; }
        }

        public void Initialize()
        {
            if (!initialized)
            {
                lock (SyncRoot)
                {
                    if (!initialized)
                    {
                        DoInit();
                    }
                }
            }
        }

        private void DoInit()
        {
            InitPopulations();
            initialized = true;
            currentIteration = 0;
            bestEntity = null;
        }

        public void Uninitialize()
        {
            if (!initialized)
            {
                lock (SyncRoot)
                {
                    if (!initialized)
                    {
                        populations = null;
                        initialized = false;
                        currentIteration = 0;
                        bestEntity = null;
                    }
                }
            }
        }

        public void Step()
        {
            lock (SyncRoot)
            {
                // Ensure init:
                if (!initialized) DoInit();

                GAIteration();
                SelectBest();
                currentIteration++;
            }
        }

        #endregion

        #region Entity Find

        private void SelectBest()
        {
            var best = populations[0].Keys[0];
            if (populations.Length > 1)
            {
                for (int idx = 1; idx < populations.Length; idx++)
                {
                    var next = populations[idx].Keys[0];
                    if (next.CompareTo(best) < 0)
                    {
                        best = next;
                    }
                }
            }
            bestEntity = best;
        }

        public IEnumerable<Entity<TPlan>> GetEntities()
        {
            if (populations != null)
                foreach (var pop in populations)
                    foreach (var entity in pop.Keys) yield return entity;
        }

        #endregion

        #region Init Populations

        private void InitPopulations()
        {
            populations = new Population<TPlan>[NumberOfPopulations];
            for (int idx = 0; idx < populations.Length; idx++)
            {
                populations[idx] = CreateNewPopulation();
            }
        }

        private Population<TPlan> CreateNewPopulation()
        {
            var pop = new Population<TPlan>(PopulationSize);
            foreach (var entity in EntityFactory.CreateInitialPopulation(PopulationSize))
            {
                pop.Add(entity);
            }
            return pop;
        }

        #endregion

        #region GA Iteration

        private void GAIteration()
        {
            Parallel.For(0, NumberOfPopulations, (popIdx) =>
            {
                var offspring = EntityFactory.CreateOffspring(SelectParentPlans(popIdx));
                if (offspring != null) PutOffspring(popIdx, offspring);
            });
        }

        private TPlan[] SelectParentPlans(int populationIndex)
        {
            var algo = new GaussianSelectionAlgorithm(bestSelectStdDev, SelectionDirection.FromTop);
            var parentPlans = new TPlan[numberOfParentsRange.PickRandomValue()];
            var indexes = algo.Select(IntRange.CreateExclusive(0, PopulationSize), parentPlans.Length);
            int ppidx = 0;
            var pop = populations[populationIndex];
            lock (pop)
            {
                foreach (var index in indexes)
                {
                    parentPlans[ppidx++] = pop.Keys[index].Plan;
                }
            }
            return parentPlans;
        }

        private void PutOffspring(int populationIndex, Entity<TPlan> offspring)
        {
            populationIndex = GetOffspringPopulationIndex(populationIndex);
            var algo = new GaussianSelectionAlgorithm(worstSelectStdDev, SelectionDirection.FromBottom);
            int index = algo.Select(IntRange.CreateExclusive(0, PopulationSize), 1).First(); // TODO: Optimize this!!!
            var pop = populations[populationIndex];
            lock (pop)
            {
                pop.RemoveAt(index);
                pop.Add(offspring);
            }
        }

        private int GetOffspringPopulationIndex(int populationIndex)
        {
            if (offspringMovingChance && NumberOfPopulations != 1)
            {
                if (populationIndex == 0)
                {
                    if (RandomGenerator.FiftyPercentChance)
                    {
                        return NumberOfPopulations - 1;
                    }
                    else
                    {
                        return populationIndex + 1;
                    }
                }
                else if (populationIndex == NumberOfPopulations - 1)
                {
                    if (RandomGenerator.FiftyPercentChance)
                    {
                        return 0;
                    }
                    else
                    {
                        return populationIndex - 1;
                    }
                }
                else
                {
                    if (RandomGenerator.FiftyPercentChance)
                    {
                        return populationIndex + 1;
                    }
                    else
                    {
                        return populationIndex - 1;
                    }
                }
            }
            return populationIndex;
        }

        #endregion
    }
}
