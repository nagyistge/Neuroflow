using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Diagnostics.Contracts;

namespace NeoComp.Evolution.GA
{
    [ContractClass(typeof(NaturalEntityFactoryContract<>))]
    public abstract class NaturalEntityFactory<TGene> : DNABasedEntityFactory<TGene>
    {
        #region Properties

        NaturalMutationParameters mutationParameters = new NaturalMutationParameters();

        public NaturalMutationParameters MutationParameters
        {
            get { return mutationParameters; }
        }

        IntRange validDNALenghtRange = IntRange.CreateInclusive(50, 100);

        public IntRange ValidDNALenghtRange
        {
            get { return validDNALenghtRange; }
            set
            {
                Contract.Requires(value.MinValue > 0);

                validDNALenghtRange = value;
            }
        }

        #endregion

        #region Init

        protected sealed override TGene[] CreateInitialRandomGeneSequence()
        {
            var sequence = new TGene[ValidDNALenghtRange.PickRandomValue()];
            FillInitialRandomGeneSequence(sequence);
            return sequence;
        }

        protected abstract void FillInitialRandomGeneSequence(TGene[] sequence);

        #endregion

        #region Offspring

        protected sealed override Entity<DNA<TGene>> CreateOffspring(DNA<TGene>[] inheritedDNAs)
        {
            var offspring = base.CreateOffspring(inheritedDNAs);

            if (offspring != null)
            {
                if (!ValidDNALenghtRange.IsIn(offspring.Plan.Length)) return null;
            }

            return offspring;
        }

        #endregion

        #region Mutation

        protected override IEnumerable<TGene> Mutate(IEnumerable<TGene> geneSequence)
        {
            var chunks = geneSequence.ToChunks(mutationParameters.MutationChunkSize);
            var mutated = chunks.Mutate(mutationParameters);
            var seq = mutated.ToSequence();
            var mutatedSeq = seq.Mutate(mutationParameters.PointMutationChance, GetMutatedVersion);
            return mutatedSeq;
        }

        protected abstract TGene GetMutatedVersion(TGene gene);

        private IEnumerable<TGene> GenerateRandomGeneSequence(int count)
        {
            for (int idx = 0; idx < count; idx++) yield return GetMutatedVersion(default(TGene));
        }

        #endregion
    }

    [ContractClassFor(typeof(NaturalEntityFactory<>))]
    abstract class NaturalEntityFactoryContract<TGene> : NaturalEntityFactory<TGene>
    {
        protected override TGene GetMutatedVersion(TGene gene)
        {
            return default(TGene);
        }

        protected override void FillInitialRandomGeneSequence(TGene[] sequence)
        {
            Contract.Requires(sequence != null);
            Contract.Requires(ValidDNALenghtRange.IsIn(sequence.Length));
        }

        protected override Entity<DNA<TGene>> CreateEntityInstance(DNA<TGene> dna, IEnumerable<TGene> dominantGeneSequence)
        {
            throw new NotImplementedException();
        }
    }
}
