using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using NeoComp.Core;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.GA
{
    [ContractClass(typeof(ComparableBodyTerritoryBaseContract<,>))]
    public abstract class ComparableBodyTerritoryBase<TBodyPlan, TBody> : SynchronizedObject, ITerritory<TBodyPlan, TBody>
        where TBodyPlan : class
        where TBody : class, IBody<TBodyPlan>
    {
        #region Observer

        protected class BodyObserver : IBody<TBodyPlan>
        {
            public TBody Body { get; internal set; }

            public TBodyPlan Plan
            {
                get 
                {
                    Contract.Assert(Body != null);
                    return Body.Plan; 
                }
            }

            public Guid UID
            {
                get 
                {
                    Contract.Assert(Body != null); 
                    return Body.UID; 
                }
            }

            object IBody.Plan
            {
                get 
                {
                    Contract.Assert(Body != null); 
                    return Body.Plan; 
                }
            }
        }

        #endregion

        #region Comparer

        protected abstract class BodyComparerBase : IComparer<BodyObserver>
        {
            public virtual int Compare(BodyObserver x, BodyObserver y)
            {
                return x.UID.CompareTo(y.UID);
            }
        }

        #endregion

        #region Constructor

        protected ComparableBodyTerritoryBase()
        {
            sortedBodies = new C5.TreeSet<BodyObserver>(CreateComparer());
        } 

        #endregion

        #region Invariant

        [ContractInvariantMethod]
        protected void ObjectInvariant()
        {
            Contract.Invariant(sortedBodies != null);
        }

        #endregion

        #region Fields

        C5.TreeSet<BodyObserver> sortedBodies;

        #endregion

        #region Create Comparer

        protected abstract BodyComparerBase CreateComparer();

        #endregion

        #region Create Observer

        private BodyObserver GetObserver(TBody forBody)
        {
            var obs = CreateObserver(forBody);
            obs.Body = forBody;
            return obs;
        }

        protected virtual BodyObserver CreateObserver(TBody forBody)
        {
            Contract.Requires(forBody != null);
            Contract.Ensures(Contract.Result<BodyObserver>() != null);

            return new BodyObserver();
        }

        #endregion

        #region ITerritory<TBodyPlan,TBody> Members

        public bool IsInitialized
        {
            get { return Count != 0; }
        }

        public TBody this[int index]
        {
            get { lock (SyncRoot) return sortedBodies[index].Body; }
        }

        public void Add(TBody body)
        {
            lock (SyncRoot)
            {
                sortedBodies.Add(GetObserver(body));
                if (sortedBodies[0].Body == body)
                {
                    OnBestBodyArrived(new BestBodyArrivedEventArgs<TBody>(body));
                }
            }
        }

        public void Remove(TBody body)
        {
            lock (SyncRoot)
            {
                sortedBodies.Remove(GetObserver(body));
            }
        }

        public event EventHandler<BestBodyArrivedEventArgs<TBody>> BestBodyArrived;

        protected virtual void OnBestBodyArrived(BestBodyArrivedEventArgs<TBody> e)
        {
            var handler = BestBodyArrived;
            if (handler != null) handler(this, e);
        }

        #endregion

        #region ISelectableItemCollection Members

        public int Count
        {
            get { lock (SyncRoot) return sortedBodies.Count; }
        }

        object ISelectableItemCollection.Select(int index)
        {
            return this[index];
        }

        #endregion

        #region IEnumerable<TBody> Members

        public IEnumerator<TBody> GetEnumerator()
        {
            return sortedBodies.Select(o => o.Body).GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }

    [ContractClassFor(typeof(ComparableBodyTerritoryBase<,>))]
    abstract class ComparableBodyTerritoryBaseContract<TBodyPlan, TBody> : ComparableBodyTerritoryBase<TBodyPlan, TBody>
        where TBodyPlan : class
        where TBody : class, IBody<TBodyPlan>
    {
        protected override BodyComparerBase CreateComparer()
        {
            Contract.Ensures(Contract.Result<BodyComparerBase>() != null);
            return null;
        }
    }
}
