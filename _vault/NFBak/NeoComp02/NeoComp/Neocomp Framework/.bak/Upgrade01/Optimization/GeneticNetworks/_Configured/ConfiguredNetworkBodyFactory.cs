using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Optimization.GA;
using NeoComp.Networks;
using NeoComp.Core;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.GeneticNetworks
{
    public abstract class ConfiguredNetworkBodyFactory<TParameters, TGeneData, TNetwork> : NaturalBodyFactory<ConfiguredNetworkGene<TGeneData>, ConfiguredNetworkBody<TGeneData, TNetwork>>
        where TParameters : ConfiguredNetworkParameters
        where TGeneData : struct
        where TNetwork : class, INetwork
    {
        #region Contructor

        protected ConfiguredNetworkBodyFactory(TParameters parameters)
            : base()
        {
            Contract.Requires(parameters != null);

            if (!parameters.CheckIsValid()) throw new ArgumentException("Parameters is not valid.", "parameters");
            Parameters = parameters;
        }

        protected ConfiguredNetworkBodyFactory(TParameters parameters, NaturalMutationParameters mutationParameters)
            : base(mutationParameters)
        {
            Contract.Requires(parameters != null);
            Contract.Requires(mutationParameters != null);

            if (!parameters.CheckIsValid()) throw new ArgumentException("Parameters is not valid.", "parameters");
            Parameters = parameters;
        } 

        #endregion
        
        #region Properties

        public TParameters Parameters { get; private set; }

        #endregion

        #region Point Mutation

        protected override ConfiguredNetworkGene<TGeneData> PointMutation(ConfiguredNetworkGene<TGeneData> gene)
        {
            if (Parameters.IndexMutationChance)
            {
                return new ConfiguredNetworkGene<TGeneData>(CreateAnotherConnectionIndex(gene.Index), gene.Data);
            }
            else
            {
                return new ConfiguredNetworkGene<TGeneData>(gene.Index, CreateAnotherGeneData(gene.Data));
            }
        }

        private ConnectionIndex CreateAnotherConnectionIndex(ConnectionIndex connectionIndex)
        {
            var newIdx = CreateRandomConnectionIndex();
            while (newIdx == connectionIndex) newIdx = CreateRandomConnectionIndex();
            return newIdx;
        }

        protected abstract TGeneData CreateAnotherGeneData(TGeneData geneData);

        #endregion

        #region Initial

        protected override int MaxDNASize
        {
            get { return Parameters.ConnectionCountRange.MaxValue + 1; }
        }

        protected override IEnumerable<ConfiguredNetworkGene<TGeneData>> CreateInitialGeneSequence()
        {
            int count = Parameters.ConnectionCountRange.PickRandomValue();
            for (int idx = 0; idx < count; idx++)
            {
                yield return new ConfiguredNetworkGene<TGeneData>(CreateRandomConnectionIndex(), CreateRandomGeneData());
            }
        }

        private ConnectionIndex CreateRandomConnectionIndex()
        {
            int upperIdx = CreateRandomNodeIndex();
            int lowerIdx = CreateRandomNodeIndex();
            if (Parameters.FeedForward)
            {
                while (lowerIdx <= upperIdx)
                {
                    upperIdx = CreateRandomNodeIndex();
                    lowerIdx = CreateRandomNodeIndex();
                }
            }
            return new ConnectionIndex(upperIdx, lowerIdx);
        }

        private int CreateRandomNodeIndex()
        {
            return RandomGenerator.Random.Next(Parameters.MaxConnectionIndex + 1);
        }

        protected abstract TGeneData CreateRandomGeneData();

        #endregion
    }
}
