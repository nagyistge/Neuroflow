using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.GA
{
    [ContractClass(typeof(ISelectionStrategyContract))]
    public interface ISelectionStrategy
    {
        IEnumerable Select(ISelectableItemCollection items, int count);
    }

    [ContractClassFor(typeof(ISelectionStrategy))]
    sealed class ISelectionStrategyContract : ISelectionStrategy
    {
        IEnumerable ISelectionStrategy.Select(ISelectableItemCollection items, int count)
        {
            Contract.Requires(items != null);
            Contract.Requires(count > 0);
            Contract.Ensures(Contract.Result<IEnumerable>() != null);
            return null;
        }
    }
}
