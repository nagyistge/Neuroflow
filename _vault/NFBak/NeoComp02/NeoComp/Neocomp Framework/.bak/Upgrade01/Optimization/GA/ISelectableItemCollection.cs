using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.GA
{
    [ContractClass(typeof(ISelectableItemCollectionContract))]
    public interface ISelectableItemCollection
    {
        int Count { get; }

        object Select(int index);
    }

    [ContractClassFor(typeof(ISelectableItemCollection))]
    sealed class ISelectableItemCollectionContract : ISelectableItemCollection
    {
        int ISelectableItemCollection.Count
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() >= 0);
                return 0;
            }
        }

        object ISelectableItemCollection.Select(int index)
        {
            ISelectableItemCollection sic = this;
            Contract.Requires(index >= 0 && index < sic.Count);
            Contract.Ensures(Contract.Result<object>() != null);
            return null;
        }        
    }
}
