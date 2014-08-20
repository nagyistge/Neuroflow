using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Core;
using System.Runtime.Serialization;

namespace NeoComp.Features
{
    [DataContract(IsReference = true, Namespace = NeoComp.xmlns, Name = "realFeatDesc")]
    [Serializable]
    public abstract class RealFeatureDescription : FeatureDescription
    {
        #region Constructor

        protected RealFeatureDescription(string id, DoubleRange originalValueRange, object context = null)
            : base(id, originalValueRange, context)
        {
            Contract.Requires(!string.IsNullOrEmpty(id));
            Contract.Requires(!originalValueRange.IsFixed);
        }

        #endregion

        #region Properties

        public override int OriginalValueCount
        {
            get { return 1; }
        }

        #endregion

        #region Values

        protected internal override IEnumerable<double?> GetOriginalValues(Feature feature)
        {
            yield return ((RealFeature)feature).Value;
        }

        #endregion
    }
    
    public abstract class RealFeature : Feature
    {
        protected RealFeature(RealFeatureDescription description, double? value)
            : base(description)
        {
            Contract.Requires(description != null);

            Value = value;
        }

        new public RealFeatureDescription Description
        {
            get { return (RealFeatureDescription)base.Description; }
        }

        public double? Value { get; private set; }

        public override object Data
        {
            get { return Value; }
        }
    }
}
