using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using NeoComp.Computations;
using NeoComp.Core;

namespace NeoComp.Networks.Neural
{
    public sealed class OnlineNeuralNetworkTest : ReadOnlyCollection<NeuralNetworkTest>
    {
        #region Constructor

        public OnlineNeuralNetworkTest(NeuralNetworkTest test, bool preserveTestOrder = false)
            : base(ConvertToArray(test))
        {
            Contract.Requires(test != null);

            Test = test;
            if (preserveTestOrder)
            {
                testOrder = TestArray.Select((t, idx) => new { UID = t.UID, Index = idx })
                                     .ToDictionary(info => info.UID, info => info.Index);
            }
        }

        private static NeuralNetworkTest[] ConvertToArray(NeuralNetworkTest test)
        {
            Contract.Requires(test != null);
            var testList = new List<NeuralNetworkTest>(test.TestValueUnits.Length);
            foreach (var unit in test.TestValueUnits)
            {
                var coll = new InputOutputValueUnitCollection<double, double?> { unit };
                testList.Add(new NeuralNetworkTest(coll, test.OutToInNormalizer, test.InToOutNormalizer, test.Epoch));
            }
            return testList.ToArray();
        } 

        #endregion

        #region Fields

        Dictionary<Guid, int> testOrder;

        #endregion

        #region Properties

        public NeuralNetworkTest Test { get; private set; }

        internal NeuralNetworkTest[] TestArray
        {
            get { return (NeuralNetworkTest[])Items; }
        } 

        #endregion

        #region Run

        public NeuralNetworkTestResult RunTest(NeuralNetwork network, Action<NeuralNetworkTestResult> actionBetweenTests, bool shuffle = true, int? monteCarloSelect = null)
        {
            Contract.Requires(network != null);
            Contract.Requires(actionBetweenTests != null);
            Contract.Requires(monteCarloSelect == null || monteCarloSelect.Value > 1);

            var test = Test;
            var tests = (IEnumerable<NeuralNetworkTest>)TestArray;
            var results = new List<NeuralNetworkTestResult>(TestArray.Length);

            if (shuffle) tests = tests.OrderByRandom();
            if (monteCarloSelect.HasValue) tests = tests.Take(monteCarloSelect.Value);

            lock (network)
            {
                foreach (var currentTest in tests)
                {
                    var result = currentTest.Test(network);
                    results.Add(result);
                    actionBetweenTests(result);
                }
            }

            List<ComputationTestResultEntry<double, double?>> allEntry;

            if (testOrder != null)
            {
                allEntry = (from r in results
                            from e in r
                            select new { UID = r.TestUID, Entry = e })
                                .OrderBy(info => testOrder[info.UID])
                                .Select(info => info.Entry)
                                .ToList();
            }
            else
            {
                allEntry = (from r in results
                            from e in r
                            select e).ToList();
            }

            return new NeuralNetworkTestResult(test.UID, allEntry, results[0].Computation);
        }

        #endregion
    }
}
