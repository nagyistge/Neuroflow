using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.GA
{
    public abstract class SimpleMutatorBodyFactory<TGene, TBody> : SimpleBodyFactory<TGene, TBody>
        where TBody : class, IBody<DNASequence<TGene>>
    {
        #region Properties

        Probability pointMutationChance = 0.0;

        public Probability PointMutationChance
        {
            get { lock (SyncRoot) return pointMutationChance; }
            set { lock (SyncRoot) pointMutationChance = value; }
        } 

        #endregion

        #region Mutate

        protected override void Mutate(DNASequence<TGene> dna)
        {
            dna.Mutate(PointMutationChance, DoPointMuation);
        }

        protected abstract TGene DoPointMuation(TGene gene);

        #endregion
    }
}
