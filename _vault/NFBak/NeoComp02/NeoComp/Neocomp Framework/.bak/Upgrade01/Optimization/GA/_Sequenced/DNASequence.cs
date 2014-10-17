using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.GA
{
    public static class DNASequence
    {
        public static DNASequence<TGene> Crossover<TGene>(DNASequence<TGene> dna1, DNASequence<TGene> dna2, IntRange crossoverChunkSize)
        {
            Contract.Requires(dna1 != null);
            Contract.Requires(dna2 != null);
            Contract.Requires(!crossoverChunkSize.IsZero);

            return new DNASequence<TGene>(dna1.Crossover(dna2, crossoverChunkSize));
        }
    }
    
    public sealed class DNASequence<TGene> : ReadOnlyCollection<TGene>
    {
        #region Constructors

        public DNASequence(IEnumerable<TGene> sequence)
            : base(ToList(sequence))
        {
            Contract.Requires(sequence != null);
        }

        private static List<TGene> ToList(IEnumerable<TGene> sequence)
        {
            Contract.Requires(sequence != null);
            Contract.Ensures(Contract.Result<List<TGene>>().Count > 0);
                       
            var list = sequence.ToList();
            Contract.Assume(list != null); // TODO: WTF?
            if (list.Count == 0) throw new ArgumentException("Sequence is empty.", "sequence");
            return list;
        } 

        #endregion

        #region Mutate

        public void Mutate(Probability mutationChance, Func<TGene, TGene> pointMutationMethod)
        {
            Contract.Requires(pointMutationMethod != null);
            
            if (mutationChance.IsChance)
            {
                for (int idx = 0; idx < Count; idx++)
                {
                    if (mutationChance) Items[idx] = pointMutationMethod(Items[idx]);
                }
            }
        }

        public void Mutate(Action<IList<TGene>> sequenceMutationMethod)
        {
            Contract.Requires(sequenceMutationMethod != null);
            
            sequenceMutationMethod(Items);
        }

        #endregion
    }
}
