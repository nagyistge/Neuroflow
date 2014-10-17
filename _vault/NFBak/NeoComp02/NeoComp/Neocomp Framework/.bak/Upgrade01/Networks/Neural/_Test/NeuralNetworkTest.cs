using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Computations;
using System.Diagnostics.Contracts;
using NeoComp.Core;
using System.IO;

namespace NeoComp.Networks.Neural
{
    public sealed class NeuralNetworkTest : ComputationTest<double, double?>, IComputationalNetworkTest
    {
        #region Create

        public static NeuralNetworkTest Create(FileInfo compFileInfo, bool bipolar = false, ComputationEpoch<double> epoch = null)
        {
            Contract.Requires(compFileInfo != null);
            
            using (var textReader = compFileInfo.OpenText())
            {
                var cfReader = new CompFormatReader(textReader);
                return Create(cfReader.Read(), bipolar, epoch);
            }
        }

        public static NeuralNetworkTest Create(string compDefinition, bool bipolar = false, ComputationEpoch<double> epoch = null)
        {
            Contract.Requires(!String.IsNullOrEmpty(compDefinition));

            var cfReader = new CompFormatReader(compDefinition);
            return Create(cfReader.Read(), bipolar, epoch);
        }

        public static NeuralNetworkTest Create(TextReader textReader, bool bipolar = false, ComputationEpoch<double> epoch = null, int? entryCount = null)
        {
            Contract.Requires(textReader != null);

            var cfReader = new CompFormatReader(textReader);
            return Create(cfReader.Read(entryCount), bipolar, epoch);
        }

        public static NeuralNetworkTest Create(CompFormatEntryCollection compFormatEntries, bool bipolar = false, ComputationEpoch<double> epoch = null)
        {
            Contract.Requires(!compFormatEntries.IsNullOrEmpty());

            double imin, imax;
            double? omin, omax;
            compFormatEntries.GetMinAndMaxValueInfo(out imin, out imax, out omin, out omax);

            if (!omin.HasValue) omin = 0.0;
            if (!omax.HasValue) omax = 1.0;

            double netMin = bipolar ? -1.0 : 0.0;
            var inorm = new DoubleNormalizer(imin, imax, netMin);
            var onorm = new NullableTransformer<double, double>(new DoubleNormalizer(omin.Value, omax.Value, netMin));
            var itoonorm = new DoubleNormalizer(netMin, 1.0, omin.Value, omax.Value);

            var units = CompFormatUtils.ConvertToUnits(compFormatEntries, inorm, onorm);

            return new NeuralNetworkTest(units, inorm, itoonorm, epoch);
        }

        #endregion

        #region Constructor

        public NeuralNetworkTest(
            InputOutputValueUnitCollection<double, double?> testValueUnits,
            DoubleNormalizer outToInNormalizer,
            DoubleNormalizer inToOutNormalizer,
            ComputationEpoch<double> epoch = null)
            : base(testValueUnits, epoch)
        {
            Contract.Requires(!testValueUnits.IsNullOrEmpty());
            Contract.Requires(inToOutNormalizer != null);
            Contract.Requires(outToInNormalizer != null);

            OutToInNormalizer = outToInNormalizer;
            InToOutNormalizer = inToOutNormalizer;
            Computation = new TransformedComputation<double, double>(base.Epoch, OutToInNormalizer, InToOutNormalizer);
        } 

        #endregion

        #region Properties

        public DoubleNormalizer OutToInNormalizer { get; private set; }

        public DoubleNormalizer InToOutNormalizer { get; private set; }

        public TransformedComputation<double, double> Computation { get; private set; }

        #endregion

        #region Test

        public NeuralNetworkTestResult Test(NeuralNetwork network)
        {
            return (NeuralNetworkTestResult)base.Test(network);
        }

        protected override ComputationTestResult<double, double?> CreateResults(IList<ComputationTestResultEntry<double, double?>> entryList)
        {
            return new NeuralNetworkTestResult(UID, entryList, Computation);
        } 

        #endregion

        #region IComputationalNetworkTest Members

        IComputationTestResult IComputationalNetworkTest.Test(IComputationalNetwork network)
        {
            var unit = network.CastAs<NeuralNetwork>("network");
            return Test(unit);
        } 

        #endregion
    }
}
