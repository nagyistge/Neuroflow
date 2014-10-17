using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Diagnostics;
using System.Threading;
using System.ComponentModel;
using System.Windows;
using System.Collections;

namespace NeoComp.Optimization.GA
{
    public class PropertySuccessFactor<TContainer, TValue> : SuccessFactor<TContainer>, IWeakEventListener
        where TContainer : class
    {
        #region Constrcutor

        public PropertySuccessFactor(TContainer container, PropertyOrFieldAccessor<TContainer, TValue> accessor, ComparationMode comparationMode)
            : base(container, comparationMode)
        {
            Accessor = accessor;
            LowerIsBetter = ((ComparationMode & ComparationMode.LowerIsBetter) == ComparationMode.LowerIsBetter);
            NullIsBetter = ((ComparationMode & ComparationMode.NullIsBetter) == ComparationMode.NullIsBetter);
            Update();
            AddHandler();
        }

        #endregion

        #region Fields

        bool? generic;

        #endregion

        #region Properties

        public PropertyOrFieldAccessor<TContainer, TValue> Accessor { get; private set; }

        public TValue LastKnownValue { get; private set; }

        protected bool LowerIsBetter { get; private set; }

        protected bool NullIsBetter { get; private set; }

        #endregion

        #region Update

        public override void Update()
        {
            var sync = Container as ISynchronized;
            if (sync != null) Monitor.Enter(sync.SyncRoot);
            try
            {
                var prev = LastKnownValue;
                LastKnownValue = Accessor.Get(Container);
                LastKnownValueChanged(prev);
            }
            finally
            {
                if (sync != null) Monitor.Exit(sync.SyncRoot);
            }
        }

        protected virtual void LastKnownValueChanged(TValue oldValue)
        {
        }

        #endregion

        #region Handler

        private void AddHandler()
        {
            var npc = Container as INotifyPropertyChanged;
            if (npc != null) PropertyChangedEventManager.AddListener(npc, this, Accessor.Name);
        } 

        #endregion

        #region IWeakEventListener Members

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            return ReceiveWeakEvent(managerType);
        }

        protected virtual bool ReceiveWeakEvent(Type managerType)
        {
            if (managerType == typeof(PropertyChangedEventManager))
            {
                ToDirty();
                return true;
            }
            return false;
        }

        #endregion

        #region IComparable<ValueFactor<TContainer,TFactor>> Members

        protected override int DoCompareTo(SuccessFactor<TContainer> sfOther)
        {
            var other = (PropertySuccessFactor<TContainer, TValue>)sfOther;
            
            var myValue = LastKnownValue;
            var otherValue = other.LastKnownValue;
            bool myValueIsNull = object.ReferenceEquals(myValue, null);
            bool otherValueIsNull = object.ReferenceEquals(otherValue, null);
            
            if (!generic.HasValue && !myValueIsNull)
            {
                generic = myValue is IComparable<TValue>;
            }

            int comp;
            if (myValueIsNull)
            {
                comp = otherValueIsNull ? 0 : (NullIsBetter ? -1 : 1);
            }
            else if (otherValueIsNull)
            {
                Debug.Assert(!myValueIsNull);
                comp = NullIsBetter ? 1 : -1;
            }
            else
            {
                comp = Compare(other);
            }
            return comp;
        }

        protected virtual int Compare(PropertySuccessFactor<TContainer, TValue> other)
        {
            int comp;
            if (generic == true)
            {
                var comparable = (IComparable<TValue>)LastKnownValue;
                comp = comparable.CompareTo(other.LastKnownValue);
            }
            else
            {
                var comparable = (IComparable)LastKnownValue;
                comp = comparable.CompareTo(other.LastKnownValue);
            }
            return LowerIsBetter ? comp : -comp;
        }

        #endregion
    }
}
