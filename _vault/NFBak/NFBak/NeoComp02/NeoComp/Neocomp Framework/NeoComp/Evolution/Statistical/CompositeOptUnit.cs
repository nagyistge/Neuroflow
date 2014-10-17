using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using NeoComp.Core;

namespace NeoComp.Evolution.Statistical
{
    public abstract class CompositeOptUnit : OptUnit
    {
        protected CompositeOptUnit(string id, params OptUnit[] units)
            : this(id, (IEnumerable<OptUnit>)units)
        {
            Contract.Requires(units != null);
            Contract.Requires(units.Length > 0);
            Contract.Requires(!string.IsNullOrEmpty(id));
        }

        protected CompositeOptUnit(string id, IEnumerable<OptUnit> units)
            : base(id)
        {
            Contract.Requires(units != null);
            Contract.Requires(!string.IsNullOrEmpty(id));

            var unitList = new LinkedList<OptUnit>();
            foreach (var unit in units)
            {
                if (unit != null) unitList.AddLast(unit);
            }

            if (unitList.Count == 0) throw new InvalidOperationException("Unit collection is empty.");

            Units = ReadOnlyArray.Wrap(unitList.ToArray());
            UnitDict = Units.ToDictionary(u => u.ID);
        }

        public IList<OptUnit> Units { get; private set; }

        protected Dictionary<string, OptUnit> UnitDict { get; private set; }
    }
}
