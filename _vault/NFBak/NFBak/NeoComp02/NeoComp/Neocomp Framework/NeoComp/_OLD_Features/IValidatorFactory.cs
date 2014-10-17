using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Features
{
    [ContractClass(typeof(IValidatorFactoryContract))]
    public interface IValidatorFactory
    {
        IDataFeatureProvider BaseProvider { get; }
        
        void GetProviders(out IDataFeatureProvider trainingProvider, out IDataFeatureProvider validationProvider);
    }

    [ContractClassFor(typeof(IValidatorFactory))]
    class IValidatorFactoryContract : IValidatorFactory
    {
        void IValidatorFactory.GetProviders(out IDataFeatureProvider trainingProvider, out IDataFeatureProvider validationProvider)
        {
            Contract.Ensures(Contract.ValueAtReturn<IDataFeatureProvider>(out trainingProvider) != null);
            Contract.Ensures(Contract.ValueAtReturn<IDataFeatureProvider>(out validationProvider) != null);
            Contract.Ensures(Contract.ValueAtReturn<IDataFeatureProvider>(out trainingProvider).FeatureDescriptions == 
                Contract.ValueAtReturn<IDataFeatureProvider>(out validationProvider).FeatureDescriptions);
            trainingProvider = null;
            validationProvider = null;
        }

        IDataFeatureProvider IValidatorFactory.BaseProvider
        {
            get
            {
                Contract.Ensures(Contract.Result<IDataFeatureProvider>() != null);
                return null;
            }
        }
    }
}
