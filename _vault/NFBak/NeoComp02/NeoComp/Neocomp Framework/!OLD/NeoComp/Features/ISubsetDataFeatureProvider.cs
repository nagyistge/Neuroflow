using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Features
{
    [ContractClass(typeof(ISubsetDataFeatureProviderContract))]
    public interface ISubsetDataFeatureProvider : IDataFeatureProvider
    {
        IDataFeatureProvider GetDataSubsetProvider(params object[] subsetParameters);
    }

    [ContractClassFor(typeof(ISubsetDataFeatureProvider))]
    class ISubsetDataFeatureProviderContract : ISubsetDataFeatureProvider
    {
        IDataFeatureProvider ISubsetDataFeatureProvider.GetDataSubsetProvider(params object[] subsetParameters)
        {
            IDataFeatureProvider @this = this;
            Contract.Requires(subsetParameters != null);
            Contract.Requires(subsetParameters.Length > 0);
            Contract.Ensures(Contract.Result<IDataFeatureProvider>() != null);
            Contract.Ensures(Contract.Result<IDataFeatureProvider>().FeatureDescriptions == @this.FeatureDescriptions);
            return null;
        }

        int IDataFeatureProvider.ItemCount
        {
            get { throw new NotImplementedException(); }
        }

        FeatureDescriptionSet IFeatured.FeatureDescriptions
        {
            get { throw new NotImplementedException(); }
        }

        FeatureSet IDataFeatureProvider.this[int index]
        {
            get { throw new NotImplementedException(); }
        }

        IList<FeatureSet> IDataFeatureProvider.GetItems(int index, int count)
        {
            throw new NotImplementedException();
        }

        IList<FeatureSet> IDataFeatureProvider.GetAllItems()
        {
            throw new NotImplementedException();
        }
    }
}
