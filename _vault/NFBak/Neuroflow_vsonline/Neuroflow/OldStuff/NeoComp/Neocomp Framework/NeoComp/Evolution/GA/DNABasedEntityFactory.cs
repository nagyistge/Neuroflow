using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Diagnostics;
using NeoComp.Core;

namespace NeoComp.Evolution.GA
{
    [ContractClass(typeof(DNABasedEntityFactoryContract<>))]
    public abstract class DNABasedEntityFactory<TGene> : IGAEntityFactory<DNA<TGene>>
    {
        #region Properties

        public bool StoreParentSequences { get; set; }

        IntRange crossoverChunkSize = IntRange.CreateInclusive(5, 10);

        public IntRange CrossoverChunkSize
        {
            get { return crossoverChunkSize; }
            set
            {
                Contract.Requires(value.MinValue > 0);

                crossoverChunkSize = value;
            }
        }

        #endregion

        #region Creation

        private Entity<DNA<TGene>>[] CreateInitialPopulation(int entityCount)
        {
            Contract.Requires(entityCount > 0);
            Contract.Ensures(Contract.Result<Entity<DNA<TGene>>[]>() != null);
            Contract.Ensures(Contract.Result<Entity<DNA<TGene>>[]>().Length == entityCount);

            var entities = new Entity<DNA<TGene>>[entityCount];
            for (int idx = 0; idx < entityCount; idx++)
            {
                var dna = CreateInitialRandomDNA();
                entities[idx] = CreateEntityInstance(dna, dna.Genes[0]);
            }
            return entities;
        }

        private DNA<TGene> CreateInitialRandomDNA()
        {
            Contract.Ensures(Contract.Result<DNA<TGene>>() != null);
            Contract.Ensures(Contract.Result<DNA<TGene>>().Length != 0);

            return new DNA<TGene>(CreateInitialRandomGeneSequence());
        }

        protected abstract TGene[] CreateInitialRandomGeneSequence();

        #endregion

        #region Offspring

        protected virtual Entity<DNA<TGene>> CreateOffspring(DNA<TGene>[] inheritedDNAs)
        {
            Contract.Requires(inheritedDNAs != null);
            Contract.Requires(inheritedDNAs.Length != 0);

            // Can return null!

            int maxLength = 0;
            var seedSequences = new IEnumerable<TGene>[inheritedDNAs.Length];
            for (int idx = 0; idx < inheritedDNAs.Length; idx++)
            {
                var dna = inheritedDNAs[idx];
                IEnumerable<TGene> seedSeq;

                if (dna.Genes.Count == 1)
                {
                    seedSeq = inheritedDNAs[idx].Genes[0];
                }
                else
                {
                    seedSeq = Crossover(inheritedDNAs[idx].Genes);
                }

                seedSeq = Mutate(seedSeq);
                seedSequences[idx] = seedSeq;

                if (dna.Length > maxLength) maxLength = dna.Length;
            }

            IEnumerable<TGene> dominantSeq;
            DNA<TGene> offspringDNA;
            
            if (StoreParentSequences)
            {
                var seqArrayLists = new List<TGene>[inheritedDNAs.Length];
                for (int idx = 0; idx < seqArrayLists.Length; idx++)
                {
                    var geneList = new List<TGene>(maxLength);
                    var seq = seedSequences[idx];
                    foreach (var gene in seq) geneList.Add(gene);

                    if (geneList.Count != 0)
                    {
                        seqArrayLists[idx] = geneList;
                    }
                    else
                    {
                        return null;
                    }
                }
                offspringDNA = new DNA<TGene>(seqArrayLists);
                dominantSeq = Crossover(offspringDNA.Genes);
            }
            else
            {
                var geneList = new List<TGene>(maxLength);
                foreach (var gene in Crossover(seedSequences)) geneList.Add(gene);
                if (geneList.Count == 0) return null;
                offspringDNA = new DNA<TGene>(geneList);
                dominantSeq = geneList;
            }

            return CreateEntityInstance(offspringDNA, dominantSeq);
        }

        protected virtual IEnumerable<TGene> Crossover(IEnumerable<IEnumerable<TGene>> geneSequences)
        {
            Contract.Requires(geneSequences != null);
            Contract.Ensures(Contract.Result<IEnumerable<TGene>>() != null);
            
            return geneSequences.Crossover(CrossoverChunkSize);
        }

        protected virtual IEnumerable<TGene> Mutate(IEnumerable<TGene> geneSequence)
        {
            return geneSequence;
        }

        protected abstract Entity<DNA<TGene>> CreateEntityInstance(DNA<TGene> dna, IEnumerable<TGene> dominantGeneSequence);

        #endregion

        #region Factory Impl

        Entity<DNA<TGene>>[] IGAEntityFactory<DNA<TGene>>.CreateInitialPopulation(int entityCount)
        {
            return CreateInitialPopulation(entityCount);
        }

        Entity<DNA<TGene>> IGAEntityFactory<DNA<TGene>>.CreateOffspring(DNA<TGene>[] inheritedInformations)
        {
            return CreateOffspring(inheritedInformations);
        } 

        #endregion
    }

    [ContractClassFor(typeof(DNABasedEntityFactory<>))]
    abstract class DNABasedEntityFactoryContract<TGene> : DNABasedEntityFactory<TGene>
    {
        protected override TGene[] CreateInitialRandomGeneSequence()
        {
            Contract.Ensures(Contract.Result<TGene[]>() != null);
            Contract.Ensures(Contract.Result<TGene[]>().Length != 0);
            return null;
        }

        protected override Entity<DNA<TGene>> CreateEntityInstance(DNA<TGene> dna, IEnumerable<TGene> dominantGeneSequence)
        {
            Contract.Requires(dna != null);
            Contract.Requires(dna.Genes.Count != 0);
            Contract.Requires(dominantGeneSequence != null);
            Contract.Ensures(Contract.Result<Entity<DNA<TGene>>>() != null);
            return null;
        }
    }
}
