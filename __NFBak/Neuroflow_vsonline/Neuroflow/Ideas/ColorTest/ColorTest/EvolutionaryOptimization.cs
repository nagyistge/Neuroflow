using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neuroflow.Core;

namespace ColorTest
{
    public abstract class EvolutionaryOptimization
    {
        protected EvolutionaryOptimization(
            int genomLength, 
            int populationSize,
            PointMutationPars mutationPars,
            IComparer<Genom> fitnessComparer)
        {
            Contract.Requires(genomLength > 0);
            Contract.Requires(populationSize > 1);
            Contract.Requires(fitnessComparer != null);

            GenomLength = genomLength;
            PopulationSize = populationSize;
            MutationPars = mutationPars;
            FitnessComparer = new SmartComparer<Genom>(new GenomBagComparer(fitnessComparer));
        }

        protected SmartComparer<Genom> FitnessComparer { get; private set; }

        protected SortedList<Genom, int> Population { get; private set; }

        public int GenomLength { get; private set; }

        public int PopulationSize { get; private set; }

        public PointMutationPars MutationPars { get; private set; }

        public int GenerationNo { get; protected set; }

        public Genom this[int index]
        {
            get
            {
                Contract.Requires(Population == null || (Population != null && index >= 0 && index < Population.Count));

                VerifyInitialized();
                return Population.Keys[index];
            }
        }

        protected virtual void VerifyInitialized()
        {
            if (Population == null) throw new InvalidOperationException("Population is not initialized.");
        }

        public virtual void Initialize()
        {
            CreateRandomPopulation();
            GenerationNo = 1;
        }

        public virtual void NextGeneration()
        {
            VerifyInitialized();

            CreateNewPopulation();
            FitnessComparer = (SmartComparer<Genom>)Population.Comparer;

            GenerationNo++;
        }

        protected void CreateRandomPopulation()
        {
            FitnessComparer.Reset();
            Population = new SortedList<Genom, int>(PopulationSize, FitnessComparer);
            var bic = FitnessComparer.BaseComparer as IParallelInitializableGenomComprarer;
            if (bic == null)
            {
                for (int index = 0; index < PopulationSize; index++) Population.Add(Genom.CreateRandom(GenomLength), 0);
            }
            else
            {
                var children = from index in Enumerable.Range(0, PopulationSize).AsParallel()
                               select Genom.CreateRandom(GenomLength);

                foreach (var child in children.AsEnumerable())
                {
                    bic.InitializeGenomComparing(child);
                    Population.Add(child, 0);
                }
            }
        }

        protected void CreateNewPopulation()
        {
            var newFitnessComparer = (SmartComparer<Genom>)FitnessComparer.Clone();
            var newPopulation = new SortedList<Genom, int>(PopulationSize, newFitnessComparer);
            var bic = newFitnessComparer.BaseComparer as IParallelInitializableGenomComprarer;

            if (bic == null)
            {
                int index = 0;
                while (newPopulation.Count != PopulationSize)
                {
                    var childGenom = CreateChild(index++);
                    newPopulation.Add(childGenom, 0);
                }
            }
            else
            {
                var children = from index in Enumerable.Range(0, PopulationSize).AsParallel()
                               select CreateChild(index);

                foreach (var child in children.AsEnumerable())
                {
                    bic.InitializeGenomComparing(child);
                    newPopulation.Add(child, 0);
                }
            }

            Population = newPopulation;
        }

        protected virtual Genom CreateChild(int index)
        {
            throw new NotSupportedException("CreateChild method is not supported.");
        }

        protected double Mutate(double gene)
        {
            if (RandomGenerator.Random.NextDouble() <= MutationPars.Chance)
            {
                switch (MutationPars.Type)
                {
                    case PointMutationType.Gaussian:
                        gene = GetGaussianMutatedGene(gene);
                        break;
                    case PointMutationType.Uniform:
                        gene = GetUniformMutatedGene(gene);
                        break;
                    default:
                        gene = RandomGenerator.Random.NextDouble();
                        break;
                }
            }
            return gene;
        }

        private double GetUniformMutatedGene(double gene)
        {
            double originalGene = gene;
            do
            {
                gene = originalGene + (MutationPars.Strength * RandomGenerator.NextDouble(-1.0, 1.0));
            }
            while (gene < 0.0 || gene > 1.0);
            return gene;
        }

        private double GetGaussianMutatedGene(double gene)
        {
            double originalGene = gene;
            do
            {
                gene = Statistics.GenerateGauss(originalGene, MutationPars.Strength);
            }
            while (gene < 0.0 || gene > 1.0);
            return gene;
        }
    }
}
