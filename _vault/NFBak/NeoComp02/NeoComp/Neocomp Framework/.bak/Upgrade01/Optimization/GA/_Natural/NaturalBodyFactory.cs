using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Threading;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.GA
{
    [ContractClass(typeof(NaturalBodyFactoryContract<,>))]
    public abstract class NaturalBodyFactory<TGene, TBody> : BodyFactory<NaturalDNA<TGene>, TBody>
        where TBody : class, IBody<NaturalDNA<TGene>>
    {
        #region Constructors

        protected NaturalBodyFactory()
        {
            mutationParameters = new NaturalMutationParameters();
        }

        protected NaturalBodyFactory(NaturalMutationParameters parameters)
        {
            Contract.Requires(parameters != null);

            this.mutationParameters = parameters;
        }

        #endregion

        #region Invariant

        [ContractInvariantMethod]
        protected void ObjectInvariant()
        {
            Contract.Invariant(mutationParameters != null);
        }


        #endregion

        #region Properties

        NaturalMutationParameters mutationParameters;

        public NaturalMutationParameters MutationParameters
        {
            get { lock (SyncRoot) return mutationParameters; }
            set
            {
                Contract.Requires(value != null);
                lock (SyncRoot) mutationParameters = value;
            }
        }

        #endregion

        #region Crossover

        protected sealed override NaturalDNA<TGene> Crossover(NaturalDNA<TGene> plan1, NaturalDNA<TGene> plan2)
        {
            return NaturalDNA.Crossover(plan1, plan2, MutationParameters);
        } 

        #endregion

        #region Mutation

        protected sealed override NaturalDNA<TGene> Mutate(NaturalDNA<TGene> plan, bool copyNeeded)
        {
            if (copyNeeded) plan = new NaturalDNA<TGene>(plan);
            plan.Mutate(MutationParameters, PointMutation);
            return plan;
        }

        protected abstract TGene PointMutation(TGene gene);

        #endregion

        #region Initialization

        protected sealed override NaturalDNA<TGene> CreateInitialPlan()
        {
            return new NaturalDNA<TGene>(CreateInitialGeneSequence(), MaxDNASize);
        }

        protected abstract IEnumerable<TGene> CreateInitialGeneSequence();

        protected abstract int MaxDNASize { get; }

        #endregion
    }

    [ContractClassFor(typeof(NaturalBodyFactory<,>))]
    abstract class NaturalBodyFactoryContract<TGene, TBody> : NaturalBodyFactory<TGene, TBody>
        where TBody : class, IBody<NaturalDNA<TGene>>
    {
        protected override IEnumerable<TGene> CreateInitialGeneSequence()
        {
            Contract.Ensures(Contract.Result<IEnumerable<TGene>>() != null);
            return null;
        }

        protected override int MaxDNASize
        {
            get 
            {
                Contract.Ensures(Contract.Result<int>() > 0);
                return 0;
            }
        }
    }
}
