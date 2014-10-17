using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.Serialization;

namespace NeoComp.Features
{
    [DataContract(IsReference = true, Namespace = NeoComp.xmlns, Name = "valueFeatDesc")]
    [Serializable]
    public sealed class ValueFeatureDescription : RealFeatureDescription
    {
        #region Constructor

        public ValueFeatureDescription(string id, DoubleRange originalValueRange, object context = null)
            : base(id, originalValueRange, context)
        {
            Contract.Requires(!string.IsNullOrEmpty(id));
            Contract.Requires(!originalValueRange.IsFixed);
        }

        #endregion

        #region Properties

        public override int FeatureValueCount
        {
            get { return 1; }
        }

        #endregion

        #region Values

        protected internal override IEnumerable<double?> GetFeatureValues(Feature feature)
        {
            yield return ((ValueFeature)feature).Value;
        }

        #endregion

        #region Create

        public override Feature CreateFeature(object value)
        {
            if (value == null) return new ValueFeature(this, (double?)null);
            if (value is double) return new ValueFeature(this, (double)value);
            if (value is double?) return new ValueFeature(this, (double?)value);
            return new ValueFeature(this, (double?)Convert.ChangeType(value, typeof(double), CultureInfo.InvariantCulture));
        }

        protected internal override Feature CreateFeature(IEnumerator<double?> valueEnumerator, DoubleRange valueNormalizationRange)
        {
            double? value = GetNext(valueEnumerator);
            if (value.HasValue) value = (double?)OriginalValueRange.Denormalize(valueNormalizationRange.Cut(value.Value), valueNormalizationRange);
            return new ValueFeature(this, value);
        }

        #endregion
    }
    
    public sealed class ValueFeature : RealFeature
    {
        internal ValueFeature(ValueFeatureDescription description, double? value)
            : base(description, value != null ? description.OriginalValueRange.Cut(value.Value) : value)
        {
            Contract.Requires(description != null);
        }

        new public ValueFeatureDescription Description
        {
            get { return (ValueFeatureDescription)base.Description; }
        }
    }
}
