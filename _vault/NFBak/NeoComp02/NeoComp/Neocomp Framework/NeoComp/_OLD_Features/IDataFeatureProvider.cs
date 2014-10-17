using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Core;

namespace NeoComp.Features
{
    [ContractClass(typeof(IDataFeatureProviderContract))]
    public interface IDataFeatureProvider : IFeatured
    {
        int ItemCount { get; }

        FeatureSet this[int index] { get; }

        IList<FeatureSet> GetItems(int index, int count);

        IList<FeatureSet> GetAllItems();
    }

    [ContractClassFor(typeof(IDataFeatureProvider))]
    class IDataFeatureProviderContract : IDataFeatureProvider
    {
        int IDataFeatureProvider.ItemCount
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() >= 0);
                return 0;
            }
        }

        FeatureSet IDataFeatureProvider.this[int index]
        {
            get
            {
                IDataFeatureProvider p = this;
                Contract.Requires(index >= 0);
                Contract.Requires(index < p.ItemCount);
                return null;
            }
        }

        IList<FeatureSet> IDataFeatureProvider.GetItems(int index, int count)
        {
            IDataFeatureProvider p = this;
            Contract.Requires(index >= 0);
            Contract.Requires(count >= 0);
            Contract.Requires(index + count < p.ItemCount);
            Contract.Ensures(Contract.Result<IList<FeatureSet>>() != null);
            return null;
        }

        IList<FeatureSet> IDataFeatureProvider.GetAllItems()
        {
            Contract.Ensures(Contract.Result<IList<FeatureSet>>() != null);
            return null;
        }

        FeatureDescriptionSet IFeatured.FeatureDescriptions
        {
            get { throw new NotImplementedException(); }
        }
    }
}
