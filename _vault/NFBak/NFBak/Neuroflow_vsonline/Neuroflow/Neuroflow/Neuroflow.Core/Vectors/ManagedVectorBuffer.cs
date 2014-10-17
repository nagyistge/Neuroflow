using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroflow.Core.Vectors
{
    public class ManagedVectorBuffer<T> : VectorBuffer<T>
        where T : struct
    {
        protected override BufferedVector<T> DoGetOrCreate(int rowIndex, int colIndex, Func<T[]> valuesFactory)
        {
            var arr = valuesFactory();
            return CreateVector(rowIndex, colIndex, arr.Length, arr);
        }

        public T[] GetArray(BufferedVector<T> vector)
        {
            Contract.Requires(vector != null);
            Contract.Requires(vector.Owner == this);

            return (T[])vector.Data;
        }
    }
}
