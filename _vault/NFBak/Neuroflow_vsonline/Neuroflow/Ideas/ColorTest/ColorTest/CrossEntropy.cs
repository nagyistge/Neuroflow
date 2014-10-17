using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neuroflow.Core.Algorithms.Selection;
using Neuroflow.Core;

namespace ColorTest
{
    public class CrossEntropy : EvolutionaryOptimization
    {
        struct GeneEntropy
        {
            internal double Mean { get; set; }

            internal double StdDev { get; set; }

            public override string ToString()
            {
                return string.Format("Mean: {0} StdDev: {1}", Mean.ToString("0.0000"), StdDev.ToString("0.0000"));
            }
        }

        public CrossEntropy(
            int genomLength, 
            int populationSize,
            double survivalPercent,
            PointMutationPars mutationPars,
            ISelectionAlgorithm selectionAlgorithm,
            IComparer<Genom> fitnessComparer) :
            base(genomLength, populationSize, mutationPars, fitnessComparer)
        {
            Contract.Requires(genomLength > 0);
            Contract.Requires(populationSize > 1);
            Contract.Requires(survivalPercent > 0.0 && survivalPercent < 100.0);
            Contract.Requires(fitnessComparer != null);
            Contract.Requires(selectionAlgorithm != null);

            SurvivalPercent = survivalPercent;
            this.selectionAlgorithm = selectionAlgorithm;
            survivalCount = (int)Math.Round((double)PopulationSize * (SurvivalPercent / 100.0), MidpointRounding.AwayFromZero);
        }

        GeneEntropy[] genesEntropy;

        ISelectionAlgorithm selectionAlgorithm;

        int survivalCount;

        public double SurvivalPercent { get; private set; }

        protected override void VerifyInitialized()
        {
            if (genesEntropy == null) throw new InvalidOperationException("Population is not initialized.");
        }

        public override void Initialize()
        {
            base.Initialize();

            genesEntropy = new GeneEntropy[GenomLength];
            UpdateEntropy(Population.Keys);
        }

        public override void NextGeneration()
        {
            base.NextGeneration();

            var survivalIndexes = selectionAlgorithm.Select(IntRange.CreateExclusive(0, PopulationSize), survivalCount);
            UpdateEntropy(Population.Keys, survivalIndexes);
        }

        protected override Genom CreateChild(int index)
        {
            var genes = new double[GenomLength];
            for (int i = 0; i < genes.Length; i++)
            {
                var entropy = genesEntropy[i];

            GENERATE:
                double gene = Statistics.GenerateGauss(entropy.Mean, entropy.StdDev);
                if (gene < 0.0 || gene > 1.0) goto GENERATE;

                gene = Mutate(gene);

                genes[i] = gene;
            }
            return new Genom(genes);
        }

        private void UpdateEntropy(IList<Genom> genoms, IEnumerable<int> indexes = null)
        {
            Parallel.For(0, GenomLength, (i) =>
            //for (int i = 0; i < GenomLength; i++)
            {
                double mean, stdDev;
                GetGenes(genoms, indexes, i).CalculateMeanAndStdDev(out mean, out stdDev);
                genesEntropy[i] = new GeneEntropy { Mean = mean, StdDev = stdDev };
            });
        }

        private IEnumerable<double> GetGenes(IList<Genom> genoms, IEnumerable<int> indexes, int geneIndex)
        {
            if (indexes == null)
            {
                for (int i = 0; i < genoms.Count; i++)
                {
                    yield return genoms[i].Genes[geneIndex];
                }
            }
            else
            {
                foreach (int i in indexes)
                {
                    yield return genoms[i].Genes[geneIndex];
                }
            }
        }
    }
}
