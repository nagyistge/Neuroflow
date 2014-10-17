using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeoComp.Evolution.Statistical;
using System.Diagnostics.Contracts;
using NeoComp.Core;

namespace NeoComp.LogicalEvolution.Statistical
{
    [ContractClass(typeof(LogicalOptUnitContract))]
    public abstract class LogicalOptUnit : OptUnitGroup
    {
        private static IEnumerable<OptUnit> ExtendOptUnits(IEnumerable<OptUnit> units, int resolution, IntRange indexRange)
        {
            Contract.Requires(resolution > 1);
            Contract.Requires(!indexRange.IsFixed && indexRange.MinValue >= 0);
            Contract.Ensures(Contract.Result<IEnumerable<OptUnit>>() != null);

            var indexUnit = new[] { new IntegerOptUnit("Index", resolution, indexRange) };
            if (units == null)
            {
                return indexUnit;
            }
            else
            {
                return indexUnit.Concat(units);
            }
        }
        
        protected LogicalOptUnit(string id, int resolution, IntRange indexRange, IEnumerable<OptUnit> units)
            : base(id, ExtendOptUnits(units, resolution, indexRange))
        {
            Contract.Requires(!string.IsNullOrEmpty(id));
            Contract.Requires(resolution > 1);
            Contract.Requires(!indexRange.IsFixed && indexRange.MinValue >= 0);
        }

        protected internal abstract LogicalNetworkGene CreateGene(EntityDataUnit unit);

        protected int GetIndex(Dictionary<string, EntityDataUnit> registry)
        {
            Contract.Requires(registry != null);

            return GetValue<int>(registry, "Index");
        }
    }

    [ContractClassFor(typeof(LogicalOptUnit))]
    abstract class LogicalOptUnitContract : LogicalOptUnit
    {
        protected LogicalOptUnitContract()
            : base(null, 0, default(IntRange), null)
        {
        }
        
        protected internal override LogicalNetworkGene CreateGene(EntityDataUnit unit)
        {
            Contract.Requires(unit != null);
            Contract.Requires(unit.EntityData != null);
            Contract.Ensures(Contract.Result<LogicalNetworkGene>() != null);
            return null;
        }
    }
}
