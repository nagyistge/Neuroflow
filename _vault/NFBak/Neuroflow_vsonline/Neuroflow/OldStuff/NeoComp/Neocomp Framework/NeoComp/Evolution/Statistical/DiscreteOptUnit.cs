using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Core;

namespace NeoComp.Evolution.Statistical
{
    public abstract class DiscreteOptUnit<T> : OptUnit
    {
        #region Construct

        protected DiscreteOptUnit(string id, int resolution)
            : base(id)
        {
            Contract.Requires(!string.IsNullOrEmpty(id)); 
            Contract.Requires(resolution > 1);

            Resolution = resolution;
        }

        #endregion

        #region Properties

        public int Resolution { get; private set; }

        #endregion

        #region Impl

        protected internal override object CreateContext()
        {
            return null;
        }

        protected internal override EntityDataUnit CreateEntityDataUnit(object context)
        {
            if (context == null) return new EntityDataUnit(ID, CreateRandomValue());
            return new EntityDataUnit(ID, ((ItemRegistry<T>)context).Pick(CreateRandomValueExcept));
        }

        protected internal override object ComputeNewContext(IEnumerable<EntityDataUnit> eliteEntityDataUnits)
        {
            return new ItemRegistry<T>(Resolution, eliteEntityDataUnits.Select(du => du.EntityData).Cast<T>());
        }

        protected internal override EntityDataUnit CreateRandomEntityDataUnit()
        {
            return new EntityDataUnit(ID, CreateRandomValue());
        }

        #endregion

        #region Abstract

        protected abstract T CreateRandomValue();

        protected abstract T CreateRandomValueExcept(HashSet<T> items);

        #endregion
    }
}
