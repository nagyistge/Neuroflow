using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Core;
using System.Diagnostics;
using System.Threading;
using System.Diagnostics.Contracts;

namespace NeoComp.Optimization.GA
{
    public class Group<TBodyPlan, TBody> : SynchronizedObject, IGroup
        where TBodyPlan : class
        where TBody : class, IBody<TBodyPlan>
    {
        #region Constructor

        public Group(BodyFactory<TBodyPlan, TBody> bodyFactory, ITerritory<TBodyPlan, TBody> territory, int size)
        {
            Contract.Requires(bodyFactory != null);
            Contract.Requires(territory != null);
            Contract.Requires(size > 0);

            this.size = size;
            this.BodyFactory = bodyFactory;
            this.Territory = territory;
            this.parentCount = IntRange.CreateFixed(4);

            Contract.Assume(!parentCount.IsZero);

            territory.BestBodyArrived += (sender, e) =>
            {
                Contract.Requires(e != null);
                Contract.Requires(e.Body != null);
                
                OnBestBodyArrived(new BestBodyArrivedEventArgs<TBodyPlan, TBody>(e.Body));
            };
        }

        #endregion

        #region Invariant

        [ContractInvariantMethod]
        protected void ObjectInvariant()
        {
            Contract.Invariant(BodyFactory != null);
            Contract.Invariant(Territory != null);
            Contract.Invariant(size > 0);
            Contract.Invariant(!parentCount.IsZero);
            Contract.Invariant(selectionStrategy != null);
        }

        #endregion

        #region Properties

        protected ITerritory<TBodyPlan, TBody> Territory { get; private set; }

        protected BodyFactory<TBodyPlan, TBody> BodyFactory { get; private set; }

        int size;

        public int Size
        {
            get { lock (SyncRoot) return size; }
            set { lock (SyncRoot) size = value; }
        }

        IntRange parentCount;

        public IntRange ParentCount
        {
            get { lock (SyncRoot) return parentCount; }
            set { lock (SyncRoot) parentCount = value; }
        }

        ISelectionStrategy selectionStrategy = new TournamentSelectionStrategy();

        public ISelectionStrategy SelectionStrategy
        {
            get { lock (SyncRoot) return selectionStrategy; }
            set { lock (SyncRoot) selectionStrategy = value; }
        }

        #endregion

        #region Initialization

        internal void GAStarted(CancellationToken? cancellationToken)
        {
            if (!Territory.IsInitialized)
            {
                foreach (var body in BodyFactory.CreateRandomBodies(Size, cancellationToken))
                {
                    Contract.Assume(body != null); // TODO: Contracts iterator support.
                    Territory.Add(body);
                    if (cancellationToken.IsCancellationRequested()) break;
                }
            }
        }

        #endregion

        #region Generation

        internal TBody CreateOffspring(CancellationToken? cancellationToken)
        {
            int parentCount = ParentCount.PickRandomValue();
            var parents = SelectParents(parentCount);
            
            Contract.Assume(parents != null); // TODO: Contracts iterator support.

            if (cancellationToken.IsCancellationRequested())
            {
                return null;
            }

            return BodyFactory.CreateOffspring(parents, cancellationToken);
        }

        private IEnumerable<TBody> SelectParents(int parentCount)
        {
            lock (Territory.SyncRoot)
            {
                if (parentCount >= Territory.Count)
                {
                    foreach (var body in Territory) yield return body;
                }
                else
                {
                    var strategy = SelectionStrategy;
                    Contract.Assume(parentCount > 0); // TODO: Contracts iterator support.
                    var selectedBodies = strategy.Select(Territory, parentCount);
                    Contract.Assert(selectedBodies != null);
                    foreach (var bodyObj in selectedBodies)
                    {
                        var body = bodyObj as TBody;
                        if (body != null)
                        {
                            yield return body;
                        }
                        else
                        {
                            new InvalidOperationException(
                                "Selection strategy '" + strategy + " has returned an invalid object.");
                        }
                    }
                }
            }
        }

        internal void Add(TBody body)
        {
            Contract.Requires(body != null);
            
            AddInternal(body);
        }

        internal void Add(IEnumerable<TBody> bodies)
        {
            Contract.Requires(bodies != null);

            foreach (var body in bodies)
            {
                if (body != null) AddInternal(body);
            }
        }

        private void AddInternal(TBody body)
        {
            Contract.Requires(body != null); 
            
            lock (Territory.SyncRoot)
            {
                Territory.Add(body);
                TrimExcess();
            }
        }

        private void TrimExcess()
        {
            while (Territory.Count > Size)
            {
                var selected = Territory[Territory.Count - 1];
                Contract.Assume(selected != null); // TODO: Fix this, if not ITerritory indexer will crash!
                Territory.Remove(selected);
            }
        }

        #endregion

        #region Best Body

        public event EventHandler<BestBodyArrivedEventArgs<TBodyPlan, TBody>> BestBodyArrived;

        protected virtual void OnBestBodyArrived(BestBodyArrivedEventArgs<TBodyPlan, TBody> e)
        {
            var handler = BestBodyArrived;
            if (handler != null) handler(this, e);
            var helper = BestBodyArrivedHelper;
            if (helper != null) helper(this, e);
        }

        #endregion

        #region IGroup Members

        private event EventHandler<BestBodyArrivedEventArgs> BestBodyArrivedHelper;

        event EventHandler<BestBodyArrivedEventArgs> IGroup.BestBodyArrived
        {
            add { BestBodyArrivedHelper += value; }
            remove { BestBodyArrivedHelper -= value; }
        }

        #endregion
    }
}
