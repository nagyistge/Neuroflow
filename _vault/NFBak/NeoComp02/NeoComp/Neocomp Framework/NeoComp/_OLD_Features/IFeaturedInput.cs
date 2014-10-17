using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Features
{
    [ContractClass(typeof(IFeaturedInputContract))]
    public interface IFeaturedInput : IFeatured
    {
        Strings InputFeatureIDs { get; }
    }

    [ContractClassFor(typeof(IFeaturedInput))]
    class IFeaturedInputContract : IFeaturedInput
    {
        Strings IFeaturedInput.InputFeatureIDs
        {
            get
            {
                IFeaturedInput fi = this;
                Contract.Ensures(Contract.Result<Strings>() != null);
                Contract.Ensures(Contract.Result<Strings>().Count <= fi.FeatureDescriptions.Count);
                return null;
            }
        }

        FeatureDescriptionSet IFeatured.FeatureDescriptions
        {
            get { throw new NotImplementedException(); }
        }
    }
}
