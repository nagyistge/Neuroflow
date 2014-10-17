using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Neuroflow.Core.ComputationAPI
{
    public sealed class ResetDoubleValues : IReset
    {
        public ResetDoubleValues(ValueSpace<double> valueSpace, int[] indexes)
        {
            Contract.Requires(valueSpace != null);
            Contract.Requires(indexes != null);

            this.valueSpace = valueSpace;
            this.indexes = indexes;
        }

        ValueSpace<double> valueSpace;

        int[] indexes;

        public unsafe void Reset()
        {
            fixed (int* iptr = indexes)
            {
                double* vsptr = (double*)valueSpace.Ptr;

                for (int i = 0; i < indexes.Length; i++)
                {
                    int index = iptr[i];
                    vsptr[index] = 0.0;
                }
            }
        }
    }
}
