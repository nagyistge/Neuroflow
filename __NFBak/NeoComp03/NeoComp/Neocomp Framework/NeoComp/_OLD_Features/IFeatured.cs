using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Features
{
    [ContractClass(typeof(IFeaturedContract))]
    public interface IFeatured
    {
        FeatureDescriptionSet FeatureDescriptions { get; }
    }

    [ContractClassFor(typeof(IFeatured))]
    class IFeaturedContract : IFeatured
    {
        FeatureDescriptionSet IFeatured.FeatureDescriptions
        {
            get
            {
                Contract.Ensures(Contract.Result<FeatureDescriptionSet>() != null);
                Contract.Ensures(Contract.Result<FeatureDescriptionSet>().Count > 0);
                return null;
            }
        }
    }
}
