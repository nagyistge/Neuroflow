using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Computations;
using System.Diagnostics.Contracts;

namespace NeoComp.Networks.Computational.Logical
{
    public sealed class TruthTableEntry : ComputationScriptEntry<bool>
    {
        public TruthTableEntry(bool?[] inputValues, bool?[] desiredOutputValues, int numberOfIterations = 1)
            : base(inputValues, desiredOutputValues, numberOfIterations)
        {
            Contract.Requires(numberOfIterations > 0);
            Contract.Requires(inputValues == null || inputValues.Length > 0);
            Contract.Requires(desiredOutputValues == null || desiredOutputValues.Length > 0);
        }

        public TruthTableEntry(bool[] inputValues, bool[] desiredOutputValues, int numberOfIterations = 1)
            : base(inputValues.Cast<bool?>().ToArray(), desiredOutputValues.Cast<bool?>().ToArray(), numberOfIterations)
        {
            Contract.Requires(numberOfIterations > 0);
            Contract.Requires(inputValues == null || inputValues.Length > 0);
            Contract.Requires(desiredOutputValues == null || desiredOutputValues.Length > 0);
        }
    }
}
