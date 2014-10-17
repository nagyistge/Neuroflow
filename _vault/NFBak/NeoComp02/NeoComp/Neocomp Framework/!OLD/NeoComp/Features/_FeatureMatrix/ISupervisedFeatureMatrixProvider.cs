using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Computations;
using NeoComp.Core;

namespace NeoComp.Features
{
    [ContractClass(typeof(ISupervisedFeatureMatrixProviderContract))]
    public interface ISupervisedFeatureMatrixProvider : IFeatureMatrixProvider, IFeaturedInputOutput
    {
        int OutputMatrixWidth { get; }

        new SupervisedFeatureMatrix GetNext();
    }

    [ContractClassFor(typeof(ISupervisedFeatureMatrixProvider))]
    class ISupervisedFeatureMatrixProviderContract : ISupervisedFeatureMatrixProvider
    {
        int ISupervisedFeatureMatrixProvider.OutputMatrixWidth
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() > 0);
                return 0;
            }
        }

        int IFeatureMatrixProvider.MatrixWidth
        {
            get { throw new NotImplementedException(); }
        }

        SupervisedFeatureMatrix ISupervisedFeatureMatrixProvider.GetNext()
        {
            ISupervisedFeatureMatrixProvider prov = this;
            Contract.Ensures(Contract.Result<SupervisedFeatureMatrix>() != null);
            Contract.Ensures(Contract.Result<SupervisedFeatureMatrix>().Matrix.Width == prov.MatrixWidth);
            Contract.Ensures(Contract.Result<SupervisedFeatureMatrix>().Matrix.Height > 0);
            Contract.Ensures(Contract.Result<SupervisedFeatureMatrix>().OutputMatrix.Width == prov.OutputMatrixWidth);
            return default(SupervisedFeatureMatrix);
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

        Strings IFeaturedInputOutput.OutputFeatureIDs
        {
            get { throw new NotImplementedException(); }
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
