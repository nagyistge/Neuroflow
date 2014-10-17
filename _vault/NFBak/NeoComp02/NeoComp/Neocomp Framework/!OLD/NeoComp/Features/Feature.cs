using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;

namespace NeoComp.Features
{
    [DataContract(IsReference = true, Namespace = NeoComp.xmlns, Name = "featDesc")]
    [Serializable]
    public abstract class FeatureDescription
    {
        #region Constructor

        protected FeatureDescription(string id, DoubleRange originalValueRange, object context = null)
        {
            Contract.Requires(!string.IsNullOrEmpty(id));
            Contract.Requires(!originalValueRange.IsFixed);

            ID = id;
            Context = context;
            OriginalValueRange = originalValueRange;
        }

        #endregion

        #region Properties

        [DataMember(Name = "id")]
        public string ID { get; private set; }

        [DataMember(Name = "origValRng")]
        public DoubleRange OriginalValueRange { get; private set; }

        [NonSerialized]
        object context;

        public object Context
        {
            get { return context; }
            private set { context = value; }
        }

        public abstract int OriginalValueCount { get; }

        public abstract int FeatureValueCount { get; }

        #endregion

        #region Values

        protected internal abstract IEnumerable<double?> GetOriginalValues(Feature feature);

        protected internal abstract IEnumerable<double?> GetFeatureValues(Feature feature);

        internal IEnumerable<double?> GetFeatureValues(Feature feature, DoubleRange normalizationRange)
        {
            Contract.Requires(feature != null);
            Contract.Requires(!normalizationRange.IsFixed);

            foreach (double? value in GetFeatureValues(feature))
            {
                if (value.HasValue)
                {
                    yield return OriginalValueRange.Normalize(value.Value, normalizationRange);
                }
                else
                {
                    yield return null;
                }
            }
        }

        #endregion

        #region Create

        public abstract Feature CreateFeature(object value);

        protected internal abstract Feature CreateFeature(IEnumerator<double?> valueEnumerator, DoubleRange valueNormalizationRange);

        protected static double? GetNext(IEnumerator<double?> valueEnumerator)
        {
            Contract.Requires(valueEnumerator != null);

            if (valueEnumerator.MoveNext()) return valueEnumerator.Current;
            throw new InvalidOperationException("Ivalid number of feature values.");
        }

        #endregion
    }
    
    public abstract class Feature
    {
        protected Feature(FeatureDescription description)
        {
            Contract.Requires(description != null);

            Description = description;
        }
        
        public FeatureDescription Description { get; private set; }

        public abstract object Data { get; }

        public IEnumerable<double?> GetOriginalValues()
        {
            return Description.GetOriginalValues(this);
        }

        public IEnumerable<double?> GetFeatureValues()
        {
            return Description.GetFeatureValues(this);
        }

        public IEnumerable<double?> GetFeatureValues(DoubleRange normalizationRange)
        {
            Contract.Requires(!normalizationRange.IsFixed);
            return Description.GetFeatureValues(this, normalizationRange);
        }
    }
}
