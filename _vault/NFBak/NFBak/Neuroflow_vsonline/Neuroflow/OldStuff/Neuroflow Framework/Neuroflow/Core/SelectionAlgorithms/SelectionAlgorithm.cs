using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Neuroflow.Core.SelectionAlgorithms
{
    [ContractClass(typeof(SelectionAlgorithmContract))]
    public abstract class SelectionAlgorithm : ISelectionAlgorithm
    {
        public ISet<int> Select(IntRange fromRange, int count)
        {
            var result = new HashSet<int>();
            while (result.Count != count) result.Add(GetNext(fromRange, result.Count));
            return result;
        }

        protected abstract int GetNext(IntRange fromRange, int soFar);
    }

    [ContractClassFor(typeof(SelectionAlgorithm))]
    abstract class SelectionAlgorithmContract : SelectionAlgorithm
    {
        protected override int GetNext(IntRange fromRange, int soFar)
        {
            Contract.Requires(!fromRange.IsFixed);
            Contract.Requires(soFar >= 0);
            Contract.Ensures(fromRange.IsIn(Contract.Result<int>()));
            return 0;
        }
    }
}
