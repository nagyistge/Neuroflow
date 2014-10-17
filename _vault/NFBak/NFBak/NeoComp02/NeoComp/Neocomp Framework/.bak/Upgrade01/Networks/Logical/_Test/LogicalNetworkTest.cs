using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Computations;
using System.Diagnostics.Contracts;
using NeoComp.Core;
using System.IO;

namespace NeoComp.Networks.Logical
{
    public sealed class LogicalNetworkTest : ComputationTest<bool, bool?>, IComputationalNetworkTest
    {
        #region Converter

        class Converter : BooleanTransformer<double>
        {
            public override bool Transform(double value)
            {
                return value == 0.0 ? false : true;
            }
        }

        #endregion

        #region Create

        public static LogicalNetworkTest Create(FileInfo compFileInfo, ComputationEpoch<bool> epoch = null)
        {
            Contract.Requires(compFileInfo != null);

            using (var textReader = compFileInfo.OpenText())
            {
                var cfReader = new CompFormatReader(textReader);
                return Create(cfReader.Read(), epoch);
            }
        }

        public static LogicalNetworkTest Create(string compDefinition, ComputationEpoch<bool> epoch = null)
        {
            Contract.Requires(!String.IsNullOrEmpty(compDefinition));

            var cfReader = new CompFormatReader(compDefinition);
            return Create(cfReader.Read(), epoch);
        }

        public static LogicalNetworkTest Create(TextReader textReader, ComputationEpoch<bool> epoch = null, int? entryCount = null)
        {
            Contract.Requires(textReader != null);

            var cfReader = new CompFormatReader(textReader);
            return Create(cfReader.Read(entryCount), epoch);
        }

        public static LogicalNetworkTest Create(CompFormatEntryCollection compFormatEntries, ComputationEpoch<bool> epoch = null)
        {
            Contract.Requires(!compFormatEntries.IsNullOrEmpty());

            var conv = new Converter();
            var nconv = new NullableTransformer<double, bool>(conv);            

            var units = CompFormatUtils.ConvertToUnits(compFormatEntries, conv, nconv);

            return new LogicalNetworkTest(units, epoch);
        }

        #endregion

        #region Constructor

        public LogicalNetworkTest(InputOutputValueUnitCollection<bool, bool?> testValueUnits, ComputationEpoch<bool> epoch = null)
            : base(testValueUnits, epoch)
        {
            Contract.Requires(!testValueUnits.IsNullOrEmpty());
        }

        #endregion

        #region Test

        public LogicalNetworkTestResult Test(LogicalNetwork network)
        {
            return (LogicalNetworkTestResult)base.Test(network);
        }

        protected override ComputationTestResult<bool, bool?> CreateResults(IList<ComputationTestResultEntry<bool, bool?>> entryList)
        {
            return new LogicalNetworkTestResult(UID, entryList);
        }

        #endregion

        #region IComputationalNetworkTest Members

        IComputationTestResult IComputationalNetworkTest.Test(IComputationalNetwork network)
        {
            var unit = network.CastAs<LogicalNetwork>("network");
            return Test(unit);
        }

        #endregion
    }
}
