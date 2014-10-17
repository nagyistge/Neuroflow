using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Evolution.Statistical
{
    public sealed class NullOptUnit : OptUnit
    {
        public NullOptUnit(string id)
            : base(id)
        {
            Contract.Requires(!string.IsNullOrEmpty(id));
        }
        
        protected internal override object CreateContext()
        {
            return null;
        }

        protected internal override EntityDataUnit CreateEntityDataUnit(object context)
        {
            return new EntityDataUnit(ID, null);
        }

        protected internal override EntityDataUnit CreateRandomEntityDataUnit()
        {
            return new EntityDataUnit(ID, null);
        }

        protected internal override object ComputeNewContext(IEnumerable<EntityDataUnit> eliteEntityDataUnits)
        {
            return null;
        }
    }
}
