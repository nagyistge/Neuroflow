using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.SelectionAlgorithms
{
    [ContractClass(typeof(ISelectionAlgorithmContract))]
    public interface ISelectionAlgorithm
    {
        ISet<int> Select(IntRange fromRange, int count);
    }

    [ContractClassFor(typeof(ISelectionAlgorithm))]
    class ISelectionAlgorithmContract : ISelectionAlgorithm
    {
        ISet<int> ISelectionAlgorithm.Select(IntRange fromRange, int count)
        {
            Contract.Requires(!fromRange.IsFixed);
            Contract.Requires(count > 0 && fromRange.Size >= count);
            Contract.Ensures(Contract.Result<ISet<int>>() != null);
            Contract.Ensures(Contract.Result<ISet<int>>().Count == count);
            return null;
        }
    }
}
