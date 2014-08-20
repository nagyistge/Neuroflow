using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Collections;
using NeoComp.Core;

namespace NeoComp.Features
{
    [ContractClass(typeof(IFeatureDescriptionOverrideContract))]
    public interface IFeatureDescriptionOverride
    {
        IList GetDistinctValues(string featureID);

        DoubleRange? GetRange(string featureID);

        int? GetItemCount(string featureID);
    }

    [ContractClassFor(typeof(IFeatureDescriptionOverride))]
    class IFeatureDescriptionOverrideContract : IFeatureDescriptionOverride
    {
        IList IFeatureDescriptionOverride.GetDistinctValues(string featureID)
        {
            Contract.Requires(!String.IsNullOrEmpty(featureID));
            Contract.Ensures(Contract.Result<IList>() == null || Contract.Result<IList>().Count > 0);
            return null;
        }

        DoubleRange? IFeatureDescriptionOverride.GetRange(string featureID)
        {
            Contract.Requires(!String.IsNullOrEmpty(featureID));
            Contract.Ensures(Contract.Result<DoubleRange?>() == null || !Contract.Result<DoubleRange?>().Value.IsFixed);
            return null;
        }

        int? IFeatureDescriptionOverride.GetItemCount(string featureID)
        {
            Contract.Requires(!String.IsNullOrEmpty(featureID));
            Contract.Ensures(Contract.Result<int?>() == null || Contract.Result<int?>().Value > 0);
            return null;
        }
    }
}
