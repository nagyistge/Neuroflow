using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.Core.Vectors
{
    public sealed class BufferedVector<T>
        where T : struct
    {
        #region Ctor

        internal BufferedVector(VectorBuffer<T> owner, int rowIndex, int colIndex, int length, object data)
        {
            Contract.Requires(owner != null);
            Contract.Requires(rowIndex >= 0);
            Contract.Requires(colIndex >= 0);
            Contract.Requires(length > 0);

            Owner = owner;
            RowIndex = rowIndex;
            ColIndex = colIndex;
            Length = length;
            Data = data;
        }

        #endregion

        #region Props and Fields

        public VectorBuffer<T> Owner { get; private set; }

        public int RowIndex { get; private set; }

        public int ColIndex { get; private set; }

        public int Length { get; private set; }

        public object Data { get; private set; }

        #endregion
    }
}
