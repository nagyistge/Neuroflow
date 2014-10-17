using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.GA
{
    public static class NaturalDNA
    {
        public static NaturalDNA<TGene> Crossover<TGene>(NaturalDNA<TGene> dna1, NaturalDNA<TGene> dna2, NaturalMutationParameters parameters)
        {
            Contract.Requires(dna1 != null);
            Contract.Requires(dna2 != null);
            Contract.Requires(parameters != null);
            
            return new NaturalDNA<TGene>(
                dna1.Definition.Crossover(dna2.Definition, parameters.CrossoverChunkSize),
                Math.Max(dna1.MaxSize, dna2.MaxSize));
        }
    }
    
    public sealed class NaturalDNA<TGene>
    {
        #region Constructor

        public NaturalDNA(IEnumerable<TGene> definition, int maxSize)
        {
            Contract.Requires(definition != null);
            Contract.Requires(maxSize > 0);

            MaxSize = maxSize;
            this.definition = definition;
        }

        public NaturalDNA(NaturalDNA<TGene> plan)
        {
            Contract.Requires(plan != null);
            
            MaxSize = plan.MaxSize;
            if (plan.definition != null)
            {
                definition = plan.definition;
            }
            else
            {
                Debug.Assert(plan.genes != null);
                definition = plan.genes;
            }
        } 

        #endregion

        #region Properties

        public int MaxSize { get; private set; }

        IEnumerable<TGene> definition;

        internal IEnumerable<TGene> Definition
        {
            get
            {
                if (definition != null) return definition;
                Debug.Assert(genes != null);
                Contract.Assume(genes != null);
                return genes;
            }
            private set
            {
                Contract.Requires(value != null);
                definition = value;
                genes = null;
            }
        }

        ReadOnlyCollection<TGene> genes;

        public ReadOnlyCollection<TGene> Genes
        {
            get
            {
                if (genes == null)
                {
                    Debug.Assert(definition != null);
                    Contract.Assume(definition != null);
                    genes = definition.Take(MaxSize).ToList().AsReadOnly();
                    definition = null;
                }
                return genes;
            }
        } 

        #endregion

        #region Mutate

        public void Mutate(NaturalMutationParameters parameters, Func<TGene, TGene> pointMutationMethod)
        {
            Contract.Requires(parameters != null);
            Contract.Requires(pointMutationMethod != null);

            var genes = Definition;
            
            if (parameters.HasReorderMutationChance)
            {
                genes = genes.ToChunks(parameters.MutationChunkSize).Mutate(parameters).ToSequence();
            }

            genes = genes.Mutate(parameters.PointMutationChance, pointMutationMethod);

            Definition = genes;
        }

        #endregion
    }
}
