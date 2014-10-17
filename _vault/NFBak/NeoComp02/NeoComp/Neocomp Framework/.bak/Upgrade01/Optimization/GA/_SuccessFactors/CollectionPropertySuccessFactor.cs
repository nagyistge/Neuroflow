using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading;

namespace NeoComp.Optimization.GA
{
    public sealed class CollectionPropertySuccessFactor<TContainer, TValue, TItem> : PropertySuccessFactor<TContainer, TValue>
        where TContainer : class
        where TItem : class
    {
        #region Constructor

        public CollectionPropertySuccessFactor(TContainer container, PropertyOrFieldAccessor<TContainer, TValue> accessor, ComparationMode comparationMode)
            : base(container, accessor, comparationMode)
        {
        } 

        #endregion

        #region Properties

        public ObjectSuccessFactor<TItem>[] LastKnownFactors { get; private set; }

        #endregion

        #region Last Known

        protected override void LastKnownValueChanged(TValue oldValue)
        {
            var newValue = LastKnownValue;
            var oldCollection = ValueAsCollection(oldValue);
            var newCollection = ValueAsCollection(newValue);

            List<ObjectSuccessFactor<TItem>> factors = new List<ObjectSuccessFactor<TItem>>();
            if (newCollection != null)
            {
                object syncRoot = GetSyncRoot(newCollection);
                Enter(syncRoot);
                try
                {
                    foreach (TItem item in newCollection)
                    {
                        if (item != null)
                        {
                            factors.Add(SuccessFactorFactory.CreateInternal(item));
                        }
                    }
                }
                finally
                {
                    Exit(syncRoot);
                }
            }
            var oldFactors = LastKnownFactors;
            LastKnownFactors = factors.OrderBy(f => f).ToArray();
            AddHandlers(newCollection);
            RemoveHandlers(oldCollection, oldFactors);
        }

        private IEnumerable<TItem> ValueAsCollection(TValue value)
        {
            if (object.ReferenceEquals(value, null)) return null;
            if (value is IDictionary) return ((IDictionary)value).Values.Cast<TItem>();
            return value as IEnumerable<TItem>;
        }

        #endregion

        #region Handlers

        private void AddHandlers(IEnumerable newCollection)
        {
            foreach (var factor in LastKnownFactors)
            {
                if (factor != null) factor.Dirtied += new EventHandler(OnFactorDirtied);
            }
            var ncc = newCollection as INotifyCollectionChanged;
            if (ncc != null) CollectionChangedEventManager.AddListener(ncc, this);
        }

        private void RemoveHandlers(IEnumerable oldCollection, ObjectSuccessFactor<TItem>[] oldFactors)
        {
            if (!oldFactors.IsNullOrEmpty())
            {
                foreach (var factor in oldFactors)
                {
                    if (factor != null) factor.Dirtied -= new EventHandler(OnFactorDirtied);
                }
            }
            var ncc = oldCollection as INotifyCollectionChanged;
            if (ncc != null) CollectionChangedEventManager.RemoveListener(ncc, this);
        }

        void OnFactorDirtied(object sender, EventArgs e)
        {
            ToDirty();
        }

        #endregion

        #region Synchronization

        private object GetSyncRoot(IEnumerable<TItem> collection)
        {
            var list = collection as IList;
            return list != null && list.IsSynchronized ? list.SyncRoot : null;
        }

        private void Enter(object syncRoot)
        {
            if (syncRoot != null) Monitor.Enter(syncRoot);
        }

        private void Exit(object syncRoot)
        {
            if (syncRoot != null) Monitor.Exit(syncRoot);
        }

        #endregion

        #region Compare

        protected override int Compare(PropertySuccessFactor<TContainer, TValue> pOther)
        {
            var other = (CollectionPropertySuccessFactor<TContainer, TValue, TItem>)pOther;
            var myFactors = LastKnownFactors;
            var otherFactors = other.LastKnownFactors;
            Debug.Assert(myFactors != null);
            Debug.Assert(otherFactors != null);
            int count = Math.Min(myFactors.Length, otherFactors.Length);
            for (int idx = 0; idx < count; idx++)
            {
                var myFactor = myFactors[idx];
                var otherFactor = otherFactors[idx];
                bool myFactorIsNull = object.ReferenceEquals(myFactor, null);
                bool otherFactorIsNull = object.ReferenceEquals(otherFactor, null);

                int comp;
                if (myFactorIsNull)
                {
                    comp = otherFactorIsNull ? 0 : (NullIsBetter ? -1 : 1);
                }
                else if (otherFactorIsNull)
                {
                    Debug.Assert(!myFactorIsNull);
                    comp = NullIsBetter ? 1 : -1;
                }
                else
                {
                    comp = myFactor.CompareTo(otherFactor);
                }

                if (comp != 0) return comp;
            }
            return 0;
        }

        #endregion

        #region IWeakEventListener Members

        protected override bool ReceiveWeakEvent(Type managerType)
        {
            if (managerType == typeof(CollectionChangedEventManager))
            {
                ToDirty();
                return true;
            }
            return base.ReceiveWeakEvent(managerType);
        }

        #endregion
    }
}
