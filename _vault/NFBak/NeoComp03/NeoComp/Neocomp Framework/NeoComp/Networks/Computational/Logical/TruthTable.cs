using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Computations;
using System.Diagnostics.Contracts;

namespace NeoComp.Networks.Computational.Logical
{
    public sealed class TruthTable : ComputationScript<TruthTableEntry, bool>, IInterfaced
    {
        #region Constructors
        
        public TruthTable(int inputInterfaceLength, int outputInterfaceLength, IList<TruthTableEntry> entryList)
            : base(entryList)
        {
            Contract.Requires(inputInterfaceLength > 0);
            Contract.Requires(outputInterfaceLength > 0);
            Contract.Requires(entryList != null && entryList.Count > 0);
            Contract.Requires(Contract.ForAll(entryList,
                e => (e.InputVector == null || e.InputVector.Length == inputInterfaceLength) &&
                    (e.DesiredOutputVector == null || e.DesiredOutputVector.Length == outputInterfaceLength)));

            InputInterfaceLength = inputInterfaceLength;
            OutputInterfaceLength = outputInterfaceLength;
        }

        #endregion

        #region Props

        public int InputInterfaceLength { get; private set; }

        public int OutputInterfaceLength { get; private set; }

        #endregion
    }
}
