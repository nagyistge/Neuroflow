using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using NeoComp.Core;
using System.Diagnostics;

namespace NeoComp.Optimization.GA
{
    public sealed class ObjectPropertySuccessFactor<TContainer, TValue> : PropertySuccessFactor<TContainer, TValue>
        where TContainer : class
        where TValue : class
    {
        #region Constructor

        public ObjectPropertySuccessFactor(TContainer container, PropertyOrFieldAccessor<TContainer, TValue> accessor, ComparationMode comparationMode)
            : base(container, accessor, comparationMode)
        {
        } 

        #endregion

        #region Properties

        public ISuccessFactor LastKnownFactor { get; private set; }

        #endregion

        #region Last Know

        protected override void LastKnownValueChanged(TValue oldValue)
        {
            // Container is locked.
            var oldFactor = LastKnownFactor;
            var newFactor = LastKnownValue != null ? 
                (LastKnownFactor = SuccessFactorFactory.CreateInternal(LastKnownValue)) : 
                null;
            if (oldFactor != null) RemoveHandler(oldFactor);
            if (newFactor != null) AddHandler(newFactor);
        }

        private void AddHandler(ISuccessFactor factor)
        {
            factor.Dirtied += OnFactorDirtied;
        }

        private void RemoveHandler(ISuccessFactor factor)
        {
            factor.Dirtied -= OnFactorDirtied;
        }

        void OnFactorDirtied(object sender, EventArgs e)
        {
            ToDirty();
        }

        #endregion

        #region Compare

        protected override int Compare(PropertySuccessFactor<TContainer, TValue> vOther)
        {
            var other = (ObjectPropertySuccessFactor<TContainer, TValue>)vOther;
            var myLastFactor = LastKnownFactor;
            var otherLastFactor = other.LastKnownFactor;
            Debug.Assert(myLastFactor != null);
            Debug.Assert(otherLastFactor != null);
            return myLastFactor.CompareTo(otherLastFactor);
        }

        #endregion
    }
}