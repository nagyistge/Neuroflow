using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Computations;
using NeoComp.Core;

namespace NeoComp.Features
{
    [ContractClass(typeof(IFeatureMatrixProviderContract))]
    public interface IFeatureMatrixProvider : IInitializable
    {
        int MatrixWidth { get; }
        
        FeatureMatrix GetNext();
    }

    [ContractClassFor(typeof(IFeatureMatrixProvider))]
    class IFeatureMatrixProviderContract : IFeatureMatrixProvider
    {
        int IFeatureMatrixProvider.MatrixWidth
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() > 0);
                return 0;
            }
        }

        FeatureMatrix IFeatureMatrixProvider.GetNext()
        {
            IFeatureMatrixProvider prov = this;
            Contract.Ensures(Contract.Result<FeatureMatrix>() != null);
            Contract.Ensures(Contract.Result<FeatureMatrix>().Matrix.Width == prov.MatrixWidth);
            Contract.Ensures(Contract.Result<FeatureMatrix>().Matrix.Height > 0);
            return default(FeatureMatrix);
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
    }
}
