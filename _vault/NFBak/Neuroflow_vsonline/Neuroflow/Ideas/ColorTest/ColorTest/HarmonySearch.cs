using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neuroflow.Core;
using Neuroflow.Core.Algorithms.Selection;

namespace ColorTest
{
    public class HarmonySearch : EvolutionaryOptimization
    {
        public HarmonySearch(
            int genomLength, 
            int populationSize,
            double selectFromPopulationPercent,
            PointMutationPars mutationPars,
            ISelectionAlgorithm worstSelectionAlgorithm,
            IComparer<Genom> fitnessComparer) :
            base(genomLength, populationSize, mutationPars, fitnessComparer)
        {
            Contract.Requires(genomLength > 0);
            Contract.Requires(populationSize > 1);
            Contract.Requires(selectFromPopulationPercent > 0.0 && selectFromPopulationPercent < 100.0);
            Contract.Requires(fitnessComparer != null);
            Contract.Requires(worstSelectionAlgorithm != null);

            SelectFromPopulationPercent = selectFromPopulationPercent;
            this.worstSelectionAlgorithm = worstSelectionAlgorithm;
        }

        ISelectionAlgorithm worstSelectionAlgorithm;

        public double SelectFromPopulationPercent { get; private set; }

        public override void NextGeneration()
        {
            var rnd = RandomGenerator.Random;
            double[] newGenes = new double[GenomLength];
            for (int geneIndex = 0; geneIndex < newGenes.Length; geneIndex++)
            {
                if (rnd.NextDouble() < SelectFromPopulationPercent)
                {
                    int genomIndex = rnd.Next(0, PopulationSize);
                    var genom = Population.Keys[genomIndex];
                    double gene = genom.Genes[geneIndex];
                    gene = Mutate(gene);
                    newGenes[geneIndex] = gene;
                }
                else
                {
                    newGenes[geneIndex] = rnd.NextDouble();
                }
            }

            int worstGenomIndex = worstSelectionAlgorithm.Select(IntRange.CreateExclusive(0, PopulationSize), 1).First();
            var worstGenom = Population.Keys[worstGenomIndex];
            var newGenom = new Genom(newGenes);

            if (IsBetter(newGenom, worstGenom))
            {
                var pic = FitnessComparer.BaseComparer as IParallelInitializableGenomComprarer;
                Population.RemoveAt(worstGenomIndex);
                FitnessComparer.Reset();
                if (pic != null) pic.InitializeGenomComparing(newGenom);
                Population.Add(newGenom, 0);
            }

            GenerationNo++;
        }

        private bool IsBetter(Genom newGenom, Genom genom)
        {
            var cmp = (IComparer<Genom>)FitnessComparer.Clone();
            return cmp.Compare(newGenom, genom) < 0;
        }
    }
}
