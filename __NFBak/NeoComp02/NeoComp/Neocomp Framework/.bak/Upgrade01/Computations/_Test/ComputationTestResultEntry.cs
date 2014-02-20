using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Diagnostics.Contracts;

namespace NeoComp.Computations
{
    public sealed class ComputationTestResultEntry<TI, TO>
    {
        internal ComputationTestResultEntry(InputOutputValueUnit<TI, TO> testUnit, TI[] computedValues)
        {
            Contract.Requires(!testUnit.InputValues.IsNullOrEmpty());
            Contract.Requires(!testUnit.OutputValues.IsNullOrEmpty());
            Contract.Requires(!computedValues.IsNullOrEmpty());
            Contract.Requires(computedValues.Length == testUnit.OutputValues.Length);
            
            this.testUnit = testUnit;
            this.computedValues = computedValues;
        }

        InputOutputValueUnit<TI, TO> testUnit;

        public InputOutputValueUnit<TI, TO> TestUnit
        {
            get { return testUnit; }
        }

        TI[] computedValues;

        public TI[] ComputedValues
        {
            get { return computedValues; }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendValues(testUnit.InputValues);
            sb.Append(" = ");
            sb.AppendValues(testUnit.OutputValues);
            sb.Append(" | ");
            sb.AppendValues(computedValues);
            return sb.ToString();
        }
    }
}
