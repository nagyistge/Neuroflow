using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Collections.ObjectModel;

namespace NeoComp.Features
{
    public sealed class FeatureSet : KeyedCollection<string, Feature>
    {
        #region Constructors

        public FeatureSet(params Feature[] features)
            : this((IEnumerable<Feature>)features)
        {
        }

        public FeatureSet(IEnumerable<Feature> features)
        {
            if (features != null)
            {
                foreach (var f in features)
                {
                    if (f != null) Add(f);
                }
            }
        } 

        #endregion

        #region Key

        protected override string GetKeyForItem(Feature item)
        {
            return item.Description.ID;
        } 

        #endregion
    }
}
