using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Core;

namespace NeoComp.Features
{
    [ContractClass(typeof(IDataFeatureMatrixProviderContract))]
    public interface IDataFeatureMatrixProvider : IFeatureMatrixProvider, IFeaturedInput
    {
        IDataFeatureProvider DataFeatureProvider { get; }
    }

    [ContractClassFor(typeof(IDataFeatureMatrixProvider))]
    class IDataFeatureMatrixProviderContract : IDataFeatureMatrixProvider
    {
        IDataFeatureProvider IDataFeatureMatrixProvider.DataFeatureProvider
        {
            get
            {
                Contract.Ensures(Contract.Result<IDataFeatureProvider>() != null);
                return null;
            }
        }

        int IFeatureMatrixProvider.MatrixWidth
        {
            get { throw new NotImplementedException(); }
        }

        FeatureMatrix IFeatureMatrixProvider.GetNext()
        {
            throw new NotImplementedException();
        }

        bool IInitializable.Initialized
        {
            get { throw new NotImplementedException(); }
        }

        void IInitializable.Initialize()
        {
            throw new NotImplementedException();
        }

        void IInitializable.Uninitialize()
        {
            throw new NotImplementedException();
        }

        Strings IFeaturedInput.InputFeatureIDs
        {
            get { throw new NotImplementedException(); }
        }

        FeatureDescriptionSet IFeatured.FeatureDescriptions
        {
            get { throw new NotImplementedException(); }
        }
    }
}
