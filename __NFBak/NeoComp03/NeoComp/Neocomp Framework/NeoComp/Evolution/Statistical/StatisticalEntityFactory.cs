using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Core;

namespace NeoComp.Evolution.Statistical
{
    public abstract class StatisticalEntityFactory<T> : IStatisticalEntityFactory<T>
        where T : class, IComparable<T>
    {
        protected StatisticalEntityFactory(params OptUnit[] units)
            : this((IEnumerable<OptUnit>)units)
        {
            Contract.Requires(units != null);
            Contract.Requires(units.Length > 0);
        }
        
        protected StatisticalEntityFactory(IEnumerable<OptUnit> units)
        {
            Contract.Requires(units != null);

            var unitList = new LinkedList<OptUnit>();
            foreach (var unit in units)
            {
                if (unit != null) unitList.AddLast(unit);
            }

            if (unitList.Count == 0) throw new InvalidOperationException("Unit collection is empty.");

            Units = ReadOnlyArray.Wrap(unitList.ToArray());
        }
        
        public IList<OptUnit> Units { get; private set; }

        public abstract T CreateEntity(EntityDataUnit[] entityDataUnits);
    }
}
