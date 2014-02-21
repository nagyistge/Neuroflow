using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks;
using NeoComp.Optimization.GA;
using NeoComp.Core;
using System.Threading;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.GeneticNetworks
{
    [SuccessFactorContainer]
    [ContractClass(typeof(ConfiguredNetworkBodyContract<,>))]
    public abstract class ConfiguredNetworkBody<TGeneData, TNetwork> : PlannedBody<NaturalDNA<ConfiguredNetworkGene<TGeneData>>>, IInitializable
        where TGeneData : struct
        where TNetwork : class, INetwork
    {
        #region Constructors

        protected ConfiguredNetworkBody(NaturalDNA<ConfiguredNetworkGene<TGeneData>> dna, ConfiguredNetworkParameters parameters)
            : base(dna)
        {
            Contract.Requires(dna != null);
            Contract.Requires(parameters != null);

            this.parameters = parameters;
        }

        protected ConfiguredNetworkBody(Guid uid, NaturalDNA<ConfiguredNetworkGene<TGeneData>> dna, ConfiguredNetworkParameters parameters)
            : base(uid, dna)
        {
            Contract.Requires(dna != null);
            Contract.Requires(parameters != null);

            this.parameters = parameters;
        }

        #endregion

        #region Invariant

        [ContractInvariantMethod]
        new protected void ObjectInvariant()
        {
            Contract.Invariant(parameters != null);
        }

        #endregion

        #region Properties

        public abstract bool IsFunctional { get; }

        ConfiguredNetworkParameters parameters;

        protected ConfiguredNetworkParameters Parameters
        {
            get { return parameters; }
        }

        public int NodeCount { get; private set; }

        public int ConnectionCount { get; private set; }

        #endregion

        #region Factors

        [SuccessFactor(0, ComparationMode.LowerIsBetter)]
        public int NodeCountFactor
        {
            get { return IsFunctional ? NodeCount : int.MaxValue; }
        }

        [SuccessFactor(1, ComparationMode.LowerIsBetter)]
        public int ConnectionCountFactor
        {
            get { return IsFunctional ? ConnectionCount : int.MaxValue; }
        }

        [SuccessFactor(2, ComparationMode.LowerIsBetter)]
        public int PlanSizeCountFactor
        {
            get { return IsFunctional ? Plan.Genes.Count : int.MaxValue; }
        }

        #endregion

        #region Create Network

        TNetwork cachedNetwork;

        public TNetwork CreateNetwork()
        {
            Contract.Ensures(Contract.Result<TNetwork>() != null);

            lock (Parameters.SyncRoot)
            {
                if (Parameters.CacheNetwork && cachedNetwork != null) return cachedNetwork;

                var network = CreateNetworkInstance();
                InitializeNetworkInstance(network);

                if (Parameters.CacheNetwork) cachedNetwork = network;

                return network;
            }
        }

        protected virtual void InitializeNetworkInstance(TNetwork network)
        {
            Contract.Requires(network != null);

            foreach (var gene in Plan.Genes)
            {
                var connIndex = gene.Index;
                var nodeIndex = gene.NodeIndex;
                if (network[connIndex] == null)
                {
                    network.AddConnection(connIndex, CreateConnection(gene));
                }
                if (network[nodeIndex] == null)
                {
                    network.AddNode(nodeIndex, CreateNode(gene));
                }
            }
        }

        protected abstract Connection CreateConnection(ConfiguredNetworkGene<TGeneData> fromGene);

        protected abstract Node CreateNode(ConfiguredNetworkGene<TGeneData> fromGene);

        protected abstract TNetwork CreateNetworkInstance();

        #endregion

        #region IInitializable Members

        // TODO: Cancel Support

        bool isInitialized = false;

        void IInitializable.Initialize(CancellationToken? cancellationToken)
        {
            if (!isInitialized)
            {
                isInitialized = true;
                var network = CreateNetwork();
                SetupWith(network);
            }
        }

        protected virtual void SetupWith(TNetwork network)
        {
            Contract.Requires(network != null); 
            
            NodeCount = network.ConnectedNodes.Count;
            ConnectionCount = network.ConnectionCount;
        }

        #endregion
    }

    [ContractClassFor(typeof(ConfiguredNetworkBody<,>))]
    abstract class ConfiguredNetworkBodyContract<TGeneData, TNetwork> : ConfiguredNetworkBody<TGeneData, TNetwork>
        where TGeneData : struct
        where TNetwork : class, INetwork
    {
        public ConfiguredNetworkBodyContract()
            : base(null, null)
        {
        }
        
        protected override TNetwork CreateNetworkInstance()
        {
            Contract.Ensures(Contract.Result<TNetwork>() != null);
            return null;
        }

        protected override Connection CreateConnection(ConfiguredNetworkGene<TGeneData> fromGene)
        {
            Contract.Ensures(Contract.Result<Connection>() != null); 
            return null;
        }

        protected override Node CreateNode(ConfiguredNetworkGene<TGeneData> fromGene)
        {
            Contract.Ensures(Contract.Result<Node>() != null); 
            return null;
        }
    }
}
