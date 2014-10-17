using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Evolution.GA
{
    internal sealed class EntityComparer<TPlan> : IComparer<Entity<TPlan>>
        where TPlan : class
    {
        public int Compare(Entity<TPlan> x, Entity<TPlan> y)
        {
            int c = x.CompareTo(y);
            if (c != 0) return c;
            c = x.UID.CompareTo(y.UID);
            return c;
        }
    }
    
    internal class Population<TPlan> : SortedList<Entity<TPlan>, object>
        where TPlan : class
    {
        internal Population(int size)
            : base(size, new EntityComparer<TPlan>())
        {
            Contract.Requires(size > 0);
        }

        internal void Add(Entity<TPlan> entity)
        {
            Contract.Requires(entity != null);

            Add(entity, null);
        }
    }
}
