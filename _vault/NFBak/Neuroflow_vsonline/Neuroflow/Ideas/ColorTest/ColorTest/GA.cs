using Neuroflow.Core;
using Neuroflow.Core.Algorithms.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace ColorTest
{
    public class GA : EvolutionaryOptimization
    {
        public GA(
            int genomLength, 
            int populationSize, 
            int crossoverPointNum, 
            PointMutationPars mutationPars,
            ISelectionAlgorithm selectionAlgorithm,
            IComparer<Genom> fitnessComparer) :
            base(genomLength, populationSize, mutationPars, fitnessComparer)
        {
            Contract.Requires(genomLength > 0);
            Contract.Requires(populationSize > 1);
            Contract.Requires(crossoverPointNum > 0);
            Contract.Requires(fitnessComparer != null);
            Contract.Requires(selectionAlgorithm != null);

            CrossoverPointNum = crossoverPointNum;
            this.selectionAlgorithm = selectionAlgorithm;
        }

        ISelectionAlgorithm selectionAlgorithm;

        public int CrossoverPointNum { get; private set; }

        protected override Genom CreateChild(int index)
        {
            int i1, i2;
            SelectParentIndexes(out i1, out i2);

            Genom g1 = Population.Keys[i1], g2 = Population.Keys[i2];

            return CreateChild(index, g1, g2);
        }

        private Genom CreateChild(int index, Genom g1, Genom g2)
        {
            var genes = new double[GenomLength];
            var coPoints = CreateCrossoverPoints();
            bool takeFirst = true;
            for (int i = 0; i < genes.Length; i++)
            {
                if (coPoints.Contains(i)) takeFirst = !takeFirst;
                genes[i] = Mutate(takeFirst ? g1.Genes[i] : g2.Genes[i]);
            }
            return new Genom(genes);
        }

        private HashSet<int> CreateCrossoverPoints()
        {
            var rnd = RandomGenerator.Random;
            var points = new HashSet<int>();
            while (points.Count != CrossoverPointNum)
            {
                points.Add(rnd.Next(GenomLength));
            }
            return points;
        }

        private void SelectParentIndexes(out int i1, out int i2)
        {
            var indexes = selectionAlgorithm.Select(IntRange.CreateExclusive(0, PopulationSize), 2);
            i1 = -1;
            i2 = -1;
            foreach (var i in indexes)
            {
                if (i1 == -1) i1 = i; else i2 = i;
            }
        }
    }
}
