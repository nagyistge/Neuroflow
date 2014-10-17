using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Optimization.GA;
using NeoComp.Networks;
using NeoComp.Core;
using System.Threading;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.GeneticNetworks
{
    [SuccessFactorContainer]
    public abstract class AdjustedNetworkBody<TNetwork> : PlannedBody<DNASequence<double>>, IInitializable
        where TNetwork : class, INetwork
    {
        #region Constructors

        protected AdjustedNetworkBody(DNASequence<double> dna, AdjustedNetworkParameters parameters)
            : base(dna)
        {
            Contract.Requires(dna != null);
            Contract.Requires(parameters != null);

            this.parameters = parameters;
        }

        protected AdjustedNetworkBody(Guid uid, DNASequence<double> dna, AdjustedNetworkParameters parameters)
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

        AdjustedNetworkParameters parameters;

        public AdjustedNetworkParameters Parameters
        {
            get { return parameters; }
        }

        #endregion

        #region IInitializable Members

        bool isInitialized;

        void IInitializable.Initialize(CancellationToken? cancellationToken)
        {
            if (!isInitialized)
            {
                isInitialized = true;
                Initialize(cancellationToken);
            }
        }

        protected virtual void Initialize(CancellationToken? cancellationToken) { }

        #endregion
    }
}
