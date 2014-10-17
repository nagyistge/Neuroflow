using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks;
using NeoComp.Optimization.GA;
using NeoComp.Core;
using System.Threading;
using System.Diagnostics.Contracts;
using NeoComp.Computations;

namespace NeoComp.Optimization.GeneticNetworks
{
    [ContractClass(typeof(AdjustedTestableNetworkBodyContract<>))]
    public abstract class AdjustedTestableNetworkBody<TNetwork> : AdjustedNetworkBody<TNetwork>
        where TNetwork : class, IComputationalNetwork
    {
        #region Constructors

        protected AdjustedTestableNetworkBody(DNASequence<double> dna, AdjustedTestableNetworkParameters parameters)
            : base(dna, parameters)
        {
            Contract.Requires(dna != null);
            Contract.Requires(parameters != null);
        }

        protected AdjustedTestableNetworkBody(Guid uid, DNASequence<double> dna, AdjustedTestableNetworkParameters parameters)
            : base(uid, dna, parameters)
        {
            Contract.Requires(dna != null);
            Contract.Requires(parameters != null);
        }

        #endregion

        #region Properties

        new public AdjustedTestableNetworkParameters Parameters
        {
            get { return (AdjustedTestableNetworkParameters)base.Parameters; }
        }

        protected IComputationTestResult TestResult { get; private set; }

        #endregion

        #region Factors

        [SuccessFactor(0, ComparationMode.LowerIsBetter)]
        public double Error
        {
            get { return TestResult == null ? double.MaxValue : TestResult.MSE; }
        }

        #endregion

        #region Test

        // TODO: Cancel Support
        
        protected override void Initialize(CancellationToken? cancellationToken)
        {
            lock (Parameters.SyncRoot)
            {
                var network = Parameters.Network as TNetwork;
                if (network == null) throw new InvalidOperationException(ToString() + ".Parameters.Network is incompatibel with current test.");
                lock (network.SyncRoot)
                {
                    Setup(network);
                    Test(network);
                }
            }
        }

        protected abstract void Setup(TNetwork network);

        private void Test(TNetwork network)
        {
            var test = Parameters.Test;
            if (test == null) throw new InvalidOperationException(ToString() + ".Parameters.Test is null.");
            TestResult = test.Test(network);
        }

        #endregion
    }

    [ContractClassFor(typeof(AdjustedTestableNetworkBody<>))]
    abstract class AdjustedTestableNetworkBodyContract<TNetwork> : AdjustedTestableNetworkBody<TNetwork>
        where TNetwork : class, IComputationalNetwork
    {
        public AdjustedTestableNetworkBodyContract()
            : base(null, null)
        {
        }

        protected override void Setup(TNetwork network)
        {
            Contract.Requires(network != null);
        }
    }
}
