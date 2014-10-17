using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks;
using NeoComp.Optimization.GA;
using NeoComp.Core;
using System.Diagnostics.Contracts;
using NeoComp.Computations;

namespace NeoComp.Optimization.GeneticNetworks
{
    public abstract class ConfiguredTestableNetworkBody<TGeneData, TNetwork> : ConfiguredNetworkBody<TGeneData, TNetwork>
        where TGeneData : struct
        where TNetwork : class, IComputationalNetwork
    {
        #region Constructors

        protected ConfiguredTestableNetworkBody(NaturalDNA<ConfiguredNetworkGene<TGeneData>> dna, ConfiguredTestableNetworkParameters parameters)
            : base(dna, parameters)
        {
            Contract.Requires(dna != null);
            Contract.Requires(parameters != null);
        }

        protected ConfiguredTestableNetworkBody(Guid uid, NaturalDNA<ConfiguredNetworkGene<TGeneData>> dna, ConfiguredTestableNetworkParameters parameters)
            : base(uid, dna, parameters)
        {
            Contract.Requires(dna != null);
            Contract.Requires(parameters != null);
        }

        #endregion

        #region Properties

        new protected ConfiguredTestableNetworkParameters Parameters
        {
            get { return (ConfiguredTestableNetworkParameters)base.Parameters; }
        }

        public override bool IsFunctional
        {
            get { return Error <= Parameters.FunctionalErrorTreshold; }
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

        protected override void SetupWith(TNetwork network)
        {
            base.SetupWith(network);
            Test(network);
        }

        private void Test(TNetwork network)
        {
            var test = Parameters.Test;
            if (test == null) throw new InvalidOperationException(ToString() + ".Parameters.Test is null.");
            TestResult = test.Test(network);
        }

        #endregion
    }
}
