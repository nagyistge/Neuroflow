using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Evolution.GA
{
    [ContractClass(typeof(EntityContract<>))]
    public abstract class Entity<TPlan> : IComparable<Entity<TPlan>>, IEntity
        where TPlan : class
    {
        protected Entity(TPlan plan)
        {
            Contract.Requires(plan != null);

            Plan = plan;
            UID = Guid.NewGuid();
        }
        
        public TPlan Plan { get; private set; }

        public Guid UID { get; private set; }

        object IEntity.Plan
        {
            get { return Plan; }
        }

        public int CompareTo(Entity<TPlan> other)
        {
            if (other.GetType() != GetType()) throw new ArgumentException("Cannot compera to '" + other.GetType() + "' object.", "other");

            return GetComparingResult(other);
        }

        public int CompareTo(object obj)
        {
            if (obj.GetType() != GetType()) throw new ArgumentException("Cannot compera to '" + obj.GetType() + "' object.", "other");

            return GetComparingResult((Entity<TPlan>)obj);
        }

        protected abstract int GetComparingResult(Entity<TPlan> other);
    }

    [ContractClassFor(typeof(Entity<>))]
    abstract class EntityContract<T> : Entity<T>
        where T : class
    {
        protected EntityContract() : base(null)
        {
        }
        
        protected override int GetComparingResult(Entity<T> other)
        {
            Contract.Requires(other != null);
            Contract.Requires(other.GetType() == GetType());
            return 0;
        }
    }
}
