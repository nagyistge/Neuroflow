using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Diagnostics.Contracts;

namespace NeoComp.Features
{
    public sealed class SubsetValidatorFactory : IValidatorFactory
    {
        public SubsetValidatorFactory(ISubsetDataFeatureProvider baseProvider, object[] trainingProviderSubsetParameters, object[] validationProviderSubsetParameters)
        {
            Contract.Requires(baseProvider != null);
            Contract.Requires(!trainingProviderSubsetParameters.IsNullOrEmpty());
            Contract.Requires(!validationProviderSubsetParameters.IsNullOrEmpty());

            BaseProvider = baseProvider;
            TrainingProviderSubsetParameters = trainingProviderSubsetParameters;
            ValidationProviderSubsetParameters = validationProviderSubsetParameters;
        }

        public SubsetValidatorFactory(ISubsetDataFeatureProvider baseProvider, object trainingProviderSubsetParameter, object validationProviderSubsetParameter)
        {
            Contract.Requires(baseProvider != null);
            Contract.Requires(trainingProviderSubsetParameter != null);
            Contract.Requires(validationProviderSubsetParameter!= null);

            BaseProvider = baseProvider;
            TrainingProviderSubsetParameters = new[] { trainingProviderSubsetParameter };
            ValidationProviderSubsetParameters = new[] { validationProviderSubsetParameter };
        }

        public ISubsetDataFeatureProvider BaseProvider { get; private set; }

        public object[] TrainingProviderSubsetParameters { get; private set; }

        public object[] ValidationProviderSubsetParameters { get; private set; }

        public void GetProviders(out IDataFeatureProvider trainingProvider, out IDataFeatureProvider validationProvider)
        {
            try
            {
                trainingProvider = BaseProvider.GetDataSubsetProvider(TrainingProviderSubsetParameters);
                validationProvider = BaseProvider.GetDataSubsetProvider(ValidationProviderSubsetParameters);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Cannot create subset providers. See inner exception for details.", ex);
            }
        }

        IDataFeatureProvider IValidatorFactory.BaseProvider
        {
            get { return BaseProvider; }
        }
    }
}
