using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Core;
using System.Globalization;

namespace NeoComp.Computations
{
    public struct CompFormatEntry
    {
        public CompFormatEntry(double[] inputValues, double?[] outputValues)
        {
            Contract.Requires(!inputValues.IsNullOrEmpty());
            Contract.Requires(!outputValues.IsNullOrEmpty());

            this.inputValues = inputValues;
            this.outputValues = outputValues;
        }
        
        double[] inputValues;

        public double[] InputValues
        {
            get { return inputValues; }
        }

        double?[] outputValues;
        
        public double?[] OutputValues
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
