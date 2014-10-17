using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Core;
using System.Collections.ObjectModel;

namespace NeoComp.Optimization.GA
{
    public sealed class GroupCollection<TBodyPlan, TBody> : Collection<Group<TBodyPlan, TBody>>, ISynchronized, ICollection
        where TBodyPlan : class
        where TBody : class, IBody<TBodyPlan>
    {
        internal GroupCollection(Population<TBodyPlan, TBody> ownerPopulation)
        {
            Contract.Requires(ownerPopulation != null);
            Contract.Requires(ownerPopulation.SyncRoot != null);
            this.ownerPopulation = ownerPopulation;
        }

        Population<TBodyPlan, TBody> ownerPopulation;

        public object SyncRoot
        {
            get { return ownerPopulation.SyncRoot; }
        }

        internal IList<Group<TBodyPlan, TBody>> InternalList
        {
            get { return Items; }
        }

        protected override void InsertItem(int index, Group<TBodyPlan, TBody> item)
        {
            if (item == null) throw new ArgumentException("Cannot add null.");
            lock (SyncRoot)
            {
                base.InsertItem(index, item);
                ownerPopulation.GroupAdded(item);
            }
        }

        protected override void SetItem(int index, Group<TBodyPlan, TBody> item)
        {
            if (item == null) throw new ArgumentException("Cannot set to null.");
            lock (SyncRoot)
            {
                var prev = this[index];
                base.SetItem(index, item);
                if (item != prev)
                {
                    ownerPopulation.GroupRemoved(prev);
                    ownerPopulation.GroupAdded(prev);
                }
            }
        }

        protected override void RemoveItem(int index)
        {
            lock (SyncRoot)
            {
                var item = this[index];
                base.RemoveItem(index);
                ownerPopulation.GroupRemoved(item);
            }
        }

        protected override void ClearItems()
        {
            lock (SyncRoot)
            {
                var list = this.ToList();
                base.ClearItems();
                foreach (var item in list) ownerPopulation.GroupRemoved(item);
            }
        }

        bool ICollection.IsSynchronized
        {
            get { return true; }
        }

        object ICollection.SyncRoot
        {
            get { return SyncRoot; }
        }
    }
}
