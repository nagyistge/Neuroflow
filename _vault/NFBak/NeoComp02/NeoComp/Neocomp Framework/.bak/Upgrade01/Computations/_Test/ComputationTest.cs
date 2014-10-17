using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Core;

namespace NeoComp.Computations
{
    [ContractClass(typeof(ComputationTestContract<,>))]
    public abstract class ComputationTest<TI, TO> : IComputationTest<TI>
    {
        protected ComputationTest(InputOutputValueUnitCollection<TI, TO> testValueUnits, ComputationEpoch<TI> epoch = null)
        {
            Contract.Requires(!testValueUnits.IsNullOrEmpty());

            if (epoch == null) epoch = new ComputationEpoch<TI>(testValueUnits.OutputSize.Value);

            Epoch = epoch;
            TestValueUnits = testValueUnits.ToArray();
            InputSize = testValueUnits.InputSize.Value;
            OutputSize = testValueUnits.OutputSize.Value;
            UID = Guid.NewGuid();
        }

        public Guid UID { get; private set; }

        public ComputationEpoch<TI> Epoch { get; private set; }

        public int InputSize { get; private set; }

        public int OutputSize { get; private set; }

        public InputOutputValueUnit<TI, TO>[] TestValueUnits { get; private set; }

        protected ComputationTestResult<TI, TO> Test(IComputationalUnit<TI> computationalUnit)
        {
            Contract.Requires(computationalUnit != null);

            var result = new List<ComputationTestResultEntry<TI, TO>>();

            foreach (var testValueUnit in TestValueUnits)
            {
                var unitResult = new List<TI>();
                Epoch.Run(computationalUnit, testValueUnit.InputValues, unitResult);
                result.Add(new ComputationTestResultEntry<TI,TO>(testValueUnit, unitResult.ToArray()));
            }

            return CreateResults(result);
        }

        protected abstract ComputationTestResult<TI, TO> CreateResults(IList<ComputationTestResultEntry<TI, TO>> entryList);

        IComputationTestResult IComputationTest<TI>.Test(IComputationalUnit<TI> computationalUnit)
        {
            return Test(computationalUnit);
        }
    }

    [ContractClassFor(typeof(ComputationTest<,>))]
    abstract class ComputationTestContract<TI, TO> : ComputationTest<TI, TO>
    {
        protected ComputationTestContract(ComputationEpoch<TI> epoch, InputOutputValueUnitCollection<TI, TO> testValueUnits)
            : base(null, null)
        {
        }
        
        protected override ComputationTestResult<TI, TO> CreateResults(IList<ComputationTestResultEntry<TI, TO>> entryList)
        {
            Contract.Requires(!entryList.IsNullOrEmpty());
            Contract.Ensures(Contract.Result<ComputationTestResult<TI, TO>>() != null);
            return null;
        }
    }
}
