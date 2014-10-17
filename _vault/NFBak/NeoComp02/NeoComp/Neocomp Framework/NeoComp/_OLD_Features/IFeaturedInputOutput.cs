using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Features
{
    [ContractClass(typeof(IFeaturedInputOutputContract))]
    public interface IFeaturedInputOutput : IFeaturedInput
    {
        Strings OutputFeatureIDs { get; }
    }

    [ContractClassFor(typeof(IFeaturedInputOutput))]
    class IFeaturedInputOutputContract : IFeaturedInputOutput
    {
        Strings IFeaturedInputOutput.OutputFeatureIDs
        {
            get
            {
                IFeaturedInput fi = this;
                Contract.Ensures(Contract.Result<Strings>() != null);
                Contract.Ensures(Contract.Result<Strings>().Count + fi.InputFeatureIDs.Count <= fi.FeatureDescriptions.Count);
                return null;
            }
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
