using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace NeoComp.Evolution.Statistical
{
    [ContractClass(typeof(OptUnitContract))]
    public abstract class OptUnit
    {
        public OptUnit(string id)
        {
            Contract.Requires(!string.IsNullOrEmpty(id));

            ID = id;
        }
        
        public string ID { get; private set; }
        
        #region Logic

        protected internal abstract object CreateContext();

        protected internal abstract EntityDataUnit CreateEntityDataUnit(object context);

        protected internal abstract object ComputeNewContext(IEnumerable<EntityDataUnit> eliteEntityDataUnits);

        protected internal abstract EntityDataUnit CreateRandomEntityDataUnit();

        #endregion
    }

    [ContractClassFor(typeof(OptUnit))]
    abstract class OptUnitContract : OptUnit
    {
        protected OptUnitContract()
            : base(null)
        {
        }
        
        protected internal override EntityDataUnit CreateEntityDataUnit(object context)
        {
            Contract.Ensures(Contract.Result<EntityDataUnit>() != null);
            return null;
        }

        protected internal override object ComputeNewContext(IEnumerable<EntityDataUnit> eliteEntityDataUnits)
        {
            Contract.Requires(eliteEntityDataUnits != null);
            return null;
        }

        protected internal override EntityDataUnit CreateRandomEntityDataUnit()
        {
            Contract.Ensures(Contract.Result<EntityDataUnit>() != null);
            return null;
        }
    }
}
