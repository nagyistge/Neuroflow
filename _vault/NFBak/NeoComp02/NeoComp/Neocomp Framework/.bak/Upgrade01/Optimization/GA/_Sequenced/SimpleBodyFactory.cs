using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.GA
{
    [ContractClass(typeof(SimpleBodyFactoryContract<,>))]
    public abstract class SimpleBodyFactory<TGene, TBody> : BodyFactory<DNASequence<TGene>, TBody>
        where TBody : class, IBody<DNASequence<TGene>>
    {
        #region Contructor

        protected SimpleBodyFactory()
        {
            crossoverChunkSize = IntRange.CreateFixed(10);
            Contract.Assume(!crossoverChunkSize.IsZero);
        }

        #endregion

        #region Invariant

        [ContractInvariantMethod]
        protected void ObjectInvariant()
        {
            Contract.Invariant(!crossoverChunkSize.IsZero);
        }

        #endregion

        #region Properties

        IntRange crossoverChunkSize;

        public IntRange CrossoverChunkSize
        {
            get { lock (SyncRoot) return crossoverChunkSize; }
            set 
            {
                Contract.Requires(!value.IsZero);
                
                lock (SyncRoot) crossoverChunkSize = value; 
            }
        } 

        #endregion

        #region Crossover

        protected override DNASequence<TGene> Crossover(DNASequence<TGene> plan1, DNASequence<TGene> plan2)
        {
            return DNASequence.Crossover(plan1, plan2, CrossoverChunkSize);
        }

        #endregion

        #region Initialization

        protected sealed override DNASequence<TGene> CreateInitialPlan()
        {
            var seq = CreateInitialGeneSequence();
            var dna = new DNASequence<TGene>(seq);
            return dna;
        }

        protected abstract IEnumerable<TGene> CreateInitialGeneSequence();

        #endregion

        #region Mutation

        protected override DNASequence<TGene> Mutate(DNASequence<TGene> plan, bool copyNeeded)
        {
            if (copyNeeded) plan = new DNASequence<TGene>(plan.Select(item => item));
            Mutate(plan);
            return plan;
        }

        protected abstract void Mutate(DNASequence<TGene> dna);

        #endregion
    }

    [ContractClassFor(typeof(SimpleBodyFactory<,>))]
    abstract class SimpleBodyFactoryContract<TGene, TBody> : SimpleBodyFactory<TGene, TBody>
        where TBody : class, IBody<DNASequence<TGene>>
    {
        protected override IEnumerable<TGene> CreateInitialGeneSequence()
        {
            Contract.Ensures(Contract.Result<IEnumerable<TGene>>() != null); 
            return null;
        }

        protected override void Mutate(DNASequence<TGene> dna)
        {
            Contract.Requires(dna != null);
        }
    }
}
