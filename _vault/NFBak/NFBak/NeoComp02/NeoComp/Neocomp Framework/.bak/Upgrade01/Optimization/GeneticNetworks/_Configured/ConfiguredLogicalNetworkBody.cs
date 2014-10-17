using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Networks.Logical;
using NeoComp.Optimization.GA;
using NeoComp.Networks;
using NeoComp.Core;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.GeneticNetworks
{
    public class ConfiguredLogicalNetworkBody : ConfiguredTestableNetworkBody<LogicGateType, LogicalNetwork>
    {
        #region Constructors

        public ConfiguredLogicalNetworkBody(NaturalDNA<ConfiguredNetworkGene<LogicGateType>> dna, ConfiguredLogicalNetworkParameters parameters)
            : base(dna, parameters)
        {
            Contract.Requires(dna != null);
            Contract.Requires(parameters != null);
            
        }

        public ConfiguredLogicalNetworkBody(Guid uid, NaturalDNA<ConfiguredNetworkGene<LogicGateType>> dna, ConfiguredLogicalNetworkParameters parameters)
            : base(uid, dna, parameters)
        {
            Contract.Requires(dna != null);
            Contract.Requires(parameters != null);
        }

        #endregion

        #region Properties

        new protected ConfiguredLogicalNetworkParameters Parameters
        {
            get { return (ConfiguredLogicalNetworkParameters)base.Parameters; }
        }

        new public LogicalNetworkTestResult TestResult
        {
            get { return (LogicalNetworkTestResult)base.TestResult; }
        }

        #endregion

        #region Implementation

        protected override LogicalNetwork CreateNetworkInstance()
        {
            return new LogicalNetwork(Parameters.InputInterfaceLength, Parameters.OutputInterfaceLength);
        }

        protected override Connection CreateConnection(ConfiguredNetworkGene<LogicGateType> fromGene)
        {
            return new LogicalConnection();
        }

        protected override Node CreateNode(ConfiguredNetworkGene<LogicGateType> fromGene)
        {
            return new LogicGate(fromGene.Data);
        }

        #endregion
    }
}
