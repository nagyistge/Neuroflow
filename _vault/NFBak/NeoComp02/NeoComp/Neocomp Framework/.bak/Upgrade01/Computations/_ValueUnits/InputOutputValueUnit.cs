using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Core;

namespace NeoComp.Computations
{
    public sealed class InputOutputValueUnit<TI, TO>
    {
        public InputOutputValueUnit(TI[] inputValues, TO[] outputValues)
        {
            Contract.Requires(!inputValues.IsNullOrEmpty());
            Contract.Requires(!outputValues.IsNullOrEmpty());

            this.inputValues = inputValues;
            this.outputValues = outputValues;
        }

        TI[] inputValues;

        public TI[] InputValues
        {
            get { return inputValues; }
        }

        TO[] outputValues;

        public TO[] OutputValues
        {
            get { return outputValues; }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendValues(inputValues);
            sb.Append(" = ");
            sb.AppendValues(outputValues);
            return sb.ToString();
        }
    }
}
