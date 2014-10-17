using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;

namespace NeoComp.Features
{
    [CollectionDataContract(IsReference = true, Namespace = NeoComp.xmlns, Name = "featDescSet")]
    [Serializable]
    [KnownType(typeof(ValueFeatureDescription))]
    [KnownType(typeof(SetFeatureDescription))]
    public sealed class FeatureDescriptionSet : KeyedCollection<string, FeatureDescription>
    {
        #region Constructor

        public FeatureDescriptionSet()
        {
        }

        public FeatureDescriptionSet(params FeatureDescription[] featureDescriptions)
            : this((IEnumerable<FeatureDescription>)featureDescriptions)
        {
        }

        public FeatureDescriptionSet(IEnumerable<FeatureDescription> featureDescriptions)
        {
            if (featureDescriptions != null)
            {
                foreach (var fi in featureDescriptions)
                {
                    if (fi != null) Add(fi);
                }
            }
        } 

        #endregion

        #region Key

        protected override string GetKeyForItem(FeatureDescription item)
        {
            return item.ID;
        } 

        #endregion

        #region Subset

        public FeatureDescriptionSet GetSubset(Strings ids)
        {
            Contract.Requires(ids != null);

            var result = new List<FeatureDescription>();

            foreach (var id in ids)
            {
                try
                {
                    result.Add(this[id]);
                }
                catch
                {
                    throw new KeyNotFoundException("Feature info by ID '" + id + "' not found.");
                }
            }

            return new FeatureDescriptionSet(result);
        } 

        #endregion
    }
}
