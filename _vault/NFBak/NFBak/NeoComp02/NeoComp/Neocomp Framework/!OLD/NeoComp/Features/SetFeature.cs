using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Core;
using System.Collections;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace NeoComp.Features
{
    [DataContract(IsReference = true, Namespace = NeoComp.xmlns, Name = "setFeatDesc")]
    [Serializable]
    public sealed class SetFeatureDescription : RealFeatureDescription
    {
        #region Constructor

        public SetFeatureDescription(string id, bool expanded, IList items, object context = null)
            : base(id, expanded ? new DoubleRange(0.0, 1.0) : new DoubleRange(0.0, items.Count - 1), context)
        {
            Contract.Requires(!string.IsNullOrEmpty(id));
            Contract.Requires(items != null);
            Contract.Requires(items.Count > 0);

            ItemList = new ItemSL(items.Count);
            for (int idx = 0; idx < items.Count; idx++) ItemList.Add(items[idx], idx);

            IsExpanded = expanded;
        }

        #endregion

        #region Properties

        [CollectionDataContract(IsReference = true, Namespace = NeoComp.xmlns, Name = "avlItems", KeyName = "value", ValueName = "idx", ItemName = "item")]
        [Serializable]
        internal class ItemSL : SortedList<object, int>
        {
            public ItemSL()
            {
            }
            
            internal ItemSL(int cap)
                : base(cap)
            {
                Contract.Requires(cap >= 0);
            }
        }

        [DataMember(Name = "items")]
        internal ItemSL ItemList { get; private set; }

        public ReadOnlyCollection<object> Items
        {
            get { return new ReadOnlyCollection<object>(ItemList.Keys); }
        }

        [DataMember(Name = "exp")]
        public bool IsExpanded { get; private set; }

        #region Properties

        public override int FeatureValueCount
        {
            get { return IsExpanded ? ItemList.Count : 1; }
        }

        #endregion

        #endregion

        #region Value

        protected internal override IEnumerable<double?> GetFeatureValues(Feature feature)
        {
            double? value = ((SetFeature)feature).Value;
            if (IsExpanded)
            {
                if (!value.HasValue)
                {
                    for (int idx = 0; idx < ItemList.Count; idx++) yield return null;
                }
                else
                {
                    int valueIndex = (int)Math.Round(value.Value);
                    for (int idx = 0; idx < ItemList.Count; idx++)
                    {
                        if (idx == valueIndex) yield return OriginalValueRange.MaxValue; else yield return OriginalValueRange.MinValue;
                    }
                }
            }
            else
            {
                yield return value;
            }
        }

        #endregion

        #region Create

        public override Feature CreateFeature(object value)
        {
            if (value == null) return new SetFeature(this, (double?)null);
            int idx = IndexOfItem(value);
            if (idx == -1) return new SetFeature(this, (double?)null);
            return new SetFeature(this, (double?)idx);
        }

        protected internal override Feature CreateFeature(IEnumerator<double?> valueEnumerator, DoubleRange valueNormalizationRange)
        {
            double? featureValue = null;
            if (IsExpanded)
            {
                double max = valueNormalizationRange.MinValue;
                int? resultIdx = null;

                for (int idx = 0; idx < ItemList.Count; idx++)
                {
                    double? value = GetNext(valueEnumerator);
                    if (value.HasValue)
                    {
                        if (value.Value > max)
                        {
                            max = value.Value;
                            resultIdx = idx;
                        }
                    }
                }

                featureValue = resultIdx.HasValue ? (double?)resultIdx.Value : (double?)null;
            }
            else
            {
                double? value = GetNext(valueEnumerator);
                if (value.HasValue) value = (double?)OriginalValueRange.Denormalize(valueNormalizationRange.Cut(value.Value), valueNormalizationRange);
                featureValue = value;
            }
            return new SetFeature(this, featureValue);
        }

        #endregion

        #region Index Of

        public int IndexOfItem(object item)
        {
            Contract.Requires(item != null);
            
            int idx;
            if (ItemList.TryGetValue(item, out idx)) return idx;
            return -1;
        }

        #endregion
    }
    
    public sealed class SetFeature : RealFeature
    {
        internal SetFeature(SetFeatureDescription description, double? value)
            : base(description, value)
        {
            Contract.Requires(description != null);
        }

        new public SetFeatureDescription Description
        {
            get { return (SetFeatureDescription)base.Description; }
        }

        public override object Data
        {
            get
            {
                if (Value == null) return null;
                return Description.ItemList.Keys[(int)Value.Value];
            }
        }
    }
}
